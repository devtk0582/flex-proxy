using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using FlexProxy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ContentAbstraction
{
    public class FormAbstraction : IContentAbstraction
    {
        private string[] _contentTypes = { "multipart/form-data", "application/x-www-form-urlencoded" };
        private IFormCollection _internalForm;
        private IDictionary<string, List<string>> _formFieldContainer;
        private FormFileCollection _formFiles;
        private string _originalBoundary;
        private Encoding _originalEncoding;
        private IOptions<ContentModificationOptions> _options;
        private bool _isMultiPart = false;

        public string[] ContentTypes => _contentTypes;

        public FormAbstraction(IOptions<ContentModificationOptions> options)
        {
            _options = options;
        }

        public async Task<Stream> ReadAsStream()
        {
            if (_isMultiPart)
            {
                MultipartFormDataContent multipartFormDataCollection = new MultipartFormDataContent(_originalBoundary);

                foreach (var field in _formFieldContainer)
                {
                    foreach (var value in field.Value)
                    {
                        var content = new StringContent(value, _originalEncoding);

                        content.Headers.ContentType = null;

                        multipartFormDataCollection.Add(content, field.Key);
                    }
                }

                if (_formFiles != null)
                {
                    foreach (var file in _formFiles)
                    {
                        var fileStream = new StreamContent(file.OpenReadStream());

                        fileStream.Headers.ContentDisposition = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                        fileStream.Headers.ContentDisposition.FileNameStar = null;
                        fileStream.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(file.ContentType);

                        if (!string.IsNullOrEmpty(file.FileName))
                        {
                            multipartFormDataCollection.Add(fileStream, file.Name, file.FileName);
                        }
                        else
                        {
                            multipartFormDataCollection.Add(fileStream);
                        }

                    }
                }

                return await multipartFormDataCollection.ReadAsStreamAsync();
            }
            else
            {
                var formFields = new List<string>();

                foreach (var entry in _formFieldContainer)
                {
                    foreach (var value in entry.Value)
                    {
                        formFields.Add($"{entry.Key}={value}");
                    }
                }

                string formContent = string.Join("&", formFields);

                var resultStream = new MemoryStream();

                var contentBytes = _originalEncoding.GetBytes(formContent);

                await resultStream.WriteAsync(contentBytes, 0, contentBytes.Length);

                return resultStream;
            }
        }

        public async Task Register(ModificationContext modificationContext, IJSEngine engine)
        {
            FormCollection formFields = null;

            MediaTypeHeaderValue contentType;

            MediaTypeHeaderValue.TryParse(modificationContext.ContentType, out contentType);

            _originalEncoding = contentType?.Encoding ?? Encoding.UTF8;

            if (HasApplicationFormContentType(contentType))
            {
                var encoding = FilterEncoding(contentType.Encoding);

                using (var formReader = new FormReader(modificationContext.ContentStream, encoding)
                {
                    ValueCountLimit = _options.Value.FormOptions.ValueCountLimit,
                    KeyLengthLimit = _options.Value.FormOptions.KeyLengthLimit,
                    ValueLengthLimit = _options.Value.FormOptions.ValueLengthLimit,
                })
                {
                    formFields = new FormCollection(await formReader.ReadFormAsync());
                }
            }
            else if (HasMultipartFormContentType(contentType))
            {
                _isMultiPart = true;

                _originalBoundary = GetBoundary(contentType, _options.Value.FormOptions.MultipartBoundaryLengthLimit);

                formFields = await GetMultipartFormCollection(modificationContext);
            }

            _internalForm = formFields ?? FormCollection.Empty;

            if (modificationContext.ContentStream.CanSeek)
            {
                modificationContext.ContentStream.Seek(0, SeekOrigin.Begin);
            }

            _formFieldContainer = new Dictionary<string, List<string>>();

            foreach (var field in _internalForm)
            {
                foreach (var value in field.Value)
                {
                    if (_formFieldContainer.ContainsKey(field.Key))
                    {
                        _formFieldContainer[field.Key].Add(value);
                    }
                    else
                    {
                        _formFieldContainer.Add(field.Key, new List<string> { value });
                    }
                }
            }

            engine.InitializeFormApi(_formFieldContainer, _formFiles);
        }

        public async Task<bool> ValidateAsync(ModificationContext context)
        {
            return true;
        }

        private Encoding FilterEncoding(Encoding encoding)
        {
            if (encoding == null || Encoding.UTF7.Equals(encoding))
            {
                return Encoding.UTF8;
            }

            return encoding;
        }

        private string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);

            if (boundary == StringSegment.Empty)
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary.ToString();
        }

        private bool HasApplicationFormContentType(MediaTypeHeaderValue contentType)
        {
            return contentType != null && contentType.MediaType.Equals(Constants.CONTENT_TYPE_FORM_DATA_IDENTIFIER, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasMultipartFormContentType(MediaTypeHeaderValue contentType)
        {
            return contentType != null && contentType.MediaType.Equals(Constants.CONTENT_TYPE_MULTIPART_FORM_DATA_IDENTIFIER, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.ToString()) && string.IsNullOrEmpty(contentDisposition.FileNameStar.ToString());
        }

        private bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.ToString()) || !string.IsNullOrEmpty(contentDisposition.FileNameStar.ToString()));
        }

        private async Task<FormCollection> GetMultipartFormCollection(ModificationContext modificationContext)
        {
            var formAccumulator = new KeyValueAccumulator();

            var multipartReader = new MultipartReader(_originalBoundary, modificationContext.ContentStream)
            {
                HeadersCountLimit = _options.Value.FormOptions.MultipartHeadersCountLimit,
                HeadersLengthLimit = _options.Value.FormOptions.MultipartHeadersLengthLimit,
                BodyLengthLimit = _options.Value.FormOptions.MultipartBodyLengthLimit,
            };

            var section = await multipartReader.ReadNextSectionAsync();

            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (contentDisposition.IsFileDisposition())
                {
                    var fileSection = new FileMultipartSection(section, contentDisposition);

                    await section.Body.DrainAsync(new System.Threading.CancellationToken());

                    FormFile file;

                    if (contentDisposition.FileName == "\"\"")
                    {
                        file = new FormFile(
                            section.Body,
                            0,
                            section.Body.Length,
                            fileSection.Name,
                            fileSection.FileName
                        );
                    }
                    else
                    {
                        file = new FormFile(
                            section.BaseStreamOffset.HasValue ? modificationContext.ContentStream : section.Body,
                            section.BaseStreamOffset.HasValue ? section.BaseStreamOffset.Value : 0,
                            section.Body.Length,
                            fileSection.Name,
                            fileSection.FileName
                        );
                    }

                    file.Headers = new HeaderDictionary(section.Headers);

                    if (_formFiles == null)
                    {
                        _formFiles = new FormFileCollection();
                    }

                    if (_formFiles.Count >= _options.Value.FormOptions.ValueCountLimit)
                    {
                        throw new InvalidDataException($"Form value count limit {_options.Value.FormOptions.ValueCountLimit} exceeded.");
                    }

                    _formFiles.Add(file);
                }
                else if (contentDisposition.IsFormDisposition())
                {
                    var formDataSection = new FormMultipartSection(section, contentDisposition);

                    var value = await formDataSection.GetValueAsync();

                    formAccumulator.Append(formDataSection.Name, value);

                    if (formAccumulator.ValueCount > _options.Value.FormOptions.ValueCountLimit)
                    {
                        throw new InvalidDataException($"Form value count limit {_options.Value.FormOptions.ValueCountLimit} exceeded.");
                    }
                }
                else
                {
                    throw new InvalidDataException($"Unrecognized content-disposition for this section: {section.ContentDisposition}");
                }

                section = await multipartReader.ReadNextSectionAsync();
            }

            if (formAccumulator.HasValues || _formFiles != null)
            {
                return new FormCollection(formAccumulator.HasValues ? formAccumulator.GetResults() : null, _formFiles);
            }
            else
            {
                return null;
            }
        }
    }
}
