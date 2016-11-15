using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using IntelligentEditing.Models;

namespace IntelligentEditing.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload( HttpPostedFileBase upload)
        {
            var mimeTypes = new string[]{ "text/plain"};
            var minWordCount = TextFile.GetMinimumWordCount();

            if (upload == null)
            {
                ModelState.AddModelError("TextFile", "ERROR: Please upload file");
            }
            else if (!mimeTypes.Contains(upload.ContentType))
            {
                ModelState.AddModelError("TextFile", "ERROR: Please upload a valid text file.");
            }
            else
            {
                var text = new TextFile
                {
                    FileName = System.IO.Path.GetFileName(upload.FileName),
                };

                using (var reader = new StreamReader(upload.InputStream))
                {
                    text.Content = reader.ReadToEnd();
                    
                    if (!text.IsEnglish())
                        ModelState.AddModelError("TextFile", "ERROR: File contains non english characters");

                    if (text.GetWordCount() < minWordCount)
                        ModelState.AddModelError("TextFile", $"ERROR: File contains less than {minWordCount} words");

                    if (ModelState.IsValid)
                    {
                        var fileName = Path.GetFileName(upload.FileName);
                        var path = Path.Combine(GetUploadFolder(), fileName);
                        upload.SaveAs(path);

                        ViewBag.Message = $"{text.FileName} - [Is English = {text.IsEnglish().ToString()}] - Word Count - {text.GetWordCount()}] - [Repeated Phrases - {text.GetRepeatedPhraseDescription()}]";

                        RedirectToAction("Index");
                    }

                }
            }
            
                return View("Index");

        }

        private string GetUploadFolder()
        {
            return Server.MapPath(ConfigurationManager.AppSettings["Upload.Folder"]);
        }

        public ActionResult History()
        {
            var uploadedFiles = new List<TextFile>();
 
            var files = Directory.GetFiles(GetUploadFolder());
 
            foreach(var file in files)
            {
                var fileInfo = new FileInfo(file);
 
                uploadedFiles.Add ( new TextFile
                {
                    FileName = fileInfo.Name,
                    Date = fileInfo.CreationTime,
                    Content = System.IO.File.ReadAllText(fileInfo.FullName)
                });

            }

            return View(uploadedFiles);
        }

    }
}