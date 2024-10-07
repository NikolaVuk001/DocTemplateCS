using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;
using Words.CS.Constants;

namespace WordsCS
{
	public class Document : IDisposable
	{

		public Document(string pathToTemplateDoc)
		{
			PathToDoc = GenerateTempDoc(pathToTemplateDoc);

			PathToTemplate = pathToTemplateDoc;
		}

        ~Document()
		{
			Dispose();
		}



		public void FindAndReplace(string phraseToFind, string phraseToReplace, bool replaceOnlyFirstOccurence = false)
		{
			if (PathToDoc is null)
			{
				return;
			}

			using (var docWord = WordprocessingDocument.Open(PathToDoc, true))
			{

				if (docWord.MainDocumentPart is null)
				{
					throw new ArgumentNullException("Template Document Part main and/or Body is null");
				}

				var document = docWord.MainDocumentPart.Document;

				foreach (var text in document.Descendants<Text>())
				{
					if (text.Text.Contains(phraseToFind))
					{
						text.Text = text.Text.Replace(phraseToFind, phraseToReplace);
						if(replaceOnlyFirstOccurence)
						{
							break;
						}							
					}
				}
			}
		}


		public void CopyElementAfter(string startingLineOfParagraphToCopy)
		{
			if (PathToDoc is null)
			{
				return;
			}

			using (var docWord = WordprocessingDocument.Open(PathToDoc, true))
			{
				if (docWord.MainDocumentPart is null)
				{
					throw new ArgumentNullException("Template Document Part main and/or Body is null");
				}

				var document = docWord.MainDocumentPart.Document;

				var copyParagraph = document.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains(startingLineOfParagraphToCopy), null);
				
				
				if(copyParagraph is not null)
				{
					copyParagraph.InsertAfterSelf(new Paragraph(copyParagraph.OuterXml));
				}				
			}
		}

		private async Task<string> GenerateTempDoc(string pathToTemplateDoc)
		{
			if(!File.Exists(pathToTemplateDoc))
			{
				throw new FileNotFoundException($"No template found at this location: {pathToTemplateDoc}");
			}
			var fileName = Path.GetFileName(pathToTemplateDoc);
			if(!Directory.Exists(PathConstants.TempFolderPath))
			{
				Directory.CreateDirectory(PathConstants.TempFolderPath);
			}

			string destinationPath = Path.Combine(PathConstants.TempFolderPath, fileName);

			try
			{
				using(FileStream sourceStream = new FileStream(pathToTemplateDoc, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					using(FileStream destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						await sourceStream.CopyToAsync(destStream);
					}
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				throw ex;				
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return destinationPath;
		}


		public void Dispose()
		{						
			if(PathToDoc is not null)
			{
				File.Delete(PathToDoc);
			}			
		}

		public string? PathToTemplate { get; set; }
        public string? PathToDoc { get; set; }

        private string? xml_document;
		private string? xml_path;
		private string? folder_path;
			
	}
}
