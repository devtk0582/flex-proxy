using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IFormApi : IApiHostObject
    {
        void Initialize(IDictionary<string, List<string>> formFields, FormFileCollection files);
        bool HasFields();
        List<string> GetField(string key);
        void AddField(string key, string value);
        void UpdateField(string key, List<string> value);
        void DeleteField(string key);
        IFormFile GetFile(string key);
        List<IFormFile> GetFiles();
        List<IFormFile> GetFiles(string name);
        void UpdateFile(string content, string fieldName, string fileName, string contentType, string contentDisposition = null);
    }

    public class FormApi : IFormApi
    {
        private IDictionary<string, List<string>> _formFields;
        private FormFileCollection _files;

        public FormApi() { }

        public FormApi(IDictionary<string, List<string>> formFields, FormFileCollection files)
        {
            _formFields = formFields;
            _files = files;
        }

        public void Initialize(IDictionary<string, List<string>> formFields, FormFileCollection files)
        {
            _formFields = formFields;
            _files = files;
        }

        public void CleanUp()
        {
            _formFields = null;
            _files = null;
        }

        public bool HasFields()
        {
            return _formFields != null && _formFields.Count > 0;
        }

        public List<string> GetField(string key)
        {
            if (_formFields == null || !_formFields.ContainsKey(key))
            {
                return null;
            }

            return _formFields[key];
        }

        public void AddField(string key, string value)
        {
            _formFields?.Add(key, new List<string> { value });
        }

        public void UpdateField(string key, List<string> values)
        {
            if (_formFields != null)
            {
                _formFields[key] = values;
            }
        }

        public void DeleteField(string key)
        {
            _formFields?.Remove(key);
        }

        public IFormFile GetFile(string name)
        {
            return _files?.Where(file => file.Name.Contains(name)).FirstOrDefault();
        }

        public List<IFormFile> GetFiles()
        {
            return _files?.ToList();
        }

        public List<IFormFile> GetFiles(string name)
        {
            return _files?.Where(file => file.Name.Contains(name)).ToList();
        }

        private void AddFile(string content, string contentDisposition, string contentType)
        {
            var contentDispositionHeader = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(contentDisposition);

            var file = GetFormFile(content, contentDispositionHeader.Name, contentDispositionHeader.FileName, contentDisposition, contentType);

            _files?.Add(file);
        }

        public void UpdateFile(string content, string fieldName, string fileName, string contentType, string contentDisposition = null)
        {
            contentDisposition = contentDisposition ?? "form-data; name=\"" + fieldName + "\"; filename=\"" + fileName + "\"";
            var newFile = GetFormFile(content, fieldName, fileName, contentDisposition, contentType);

            var originalFile = GetFile(fieldName);

            if (originalFile != null)
            {
                _files?.Remove(originalFile);
            }

            _files?.Add(newFile);
        }

        private IFormFile GetFormFile(string content, string name, string fileName, string contentDisposition, string contentType)
        {
            var contentStream = new MemoryStream();
            var contentBytes = Convert.FromBase64String(content);
            contentStream.Write(contentBytes, 0, contentBytes.Length);

            var formFile = new FormFile(contentStream, 0, contentStream.Length, name, fileName) { Headers = new HeaderDictionary() };
            formFile.ContentDisposition = contentDisposition;
            formFile.ContentType = contentType;

            return formFile;
        }

        private void RemoveFile(string name)
        {
            _files?.RemoveAll(file => file.Name.Contains(name));
        }
    }
}
