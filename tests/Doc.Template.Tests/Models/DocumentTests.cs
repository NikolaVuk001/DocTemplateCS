using Xunit;
using WordsCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using Document = WordsCS.Document;

namespace Words.CS.Tests.Models
{
    public class DocumentTests
    {
        [Fact()]
        public void DocumentCtor_WithValidPathToFile_ShouldCreateNewDocumentInTempFolder()
        {
			// arrange
			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\Test.docx";                  

            // act
            
            Document document = new Document(pathToTemplate);

            // assert

            document.PathToTemplate.Should().Be(pathToTemplate);            
            File.Exists(@$"{Directory.GetCurrentDirectory()}\Temp\{Path.GetFileName(pathToTemplate)}").Should().BeTrue();
            File.GetAttributes(@$"{Directory.GetCurrentDirectory()}\Temp\{Path.GetFileName(pathToTemplate)}").
                GetHashCode().
                Should().
                Be(File.GetAttributes(pathToTemplate).GetHashCode());
        }

        [Fact()]
        public void DocumentCtor_WithInvalidPathToFile_ShouldThrowFileNotFoundException()
        {
            // arrange

            var pathToTemplate = "./InvalidFile.docx";

            // act

            Action action = () => { Document document = new Document(pathToTemplate); };



            // assert
            action.Should().Throw<FileNotFoundException>()
                .WithMessage($"No template found at this location: {pathToTemplate}");
        }

        [Fact()]
        public void DocumentDestructor_WithValidPathToFile_ShouldDeleteCreatedDocumentInTemp()
        {
			// arrange
			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\DestTest.docx";

			// act
			using (Document doc = new Document(pathToTemplate))
            {                           
            }
			Thread.Sleep(1000);
			System.GC.Collect();


			// assert
			File.Exists(@$"{Directory.GetCurrentDirectory()}\Temp\{Path.GetFileName(pathToTemplate)}").Should().BeFalse();

		}

        [Fact()]

        public void FindAndReplace_WithValidPhrases_ShouldChangeDocumentHash()
        {
			// arrange 

			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\Test.docx";
			var fileName = Path.GetFileName(pathToTemplate);

			Document document = new Document(pathToTemplate);

            var phraseToFind = "lorem";
            var phraseToReplace = "Changed";

            // act
            document.FindAndReplace(phraseToFind, phraseToReplace);

			// assert           			
			using (var doc = WordprocessingDocument.Open(document.PathToDoc, false))
            {
                doc.MainDocumentPart.Should().NotBeNull();

                foreach(var textElem in doc.MainDocumentPart!.Document.Descendants<Text>())
                {
					textElem.Text.Contains(phraseToFind).Should().BeFalse();
				}
                
            }

		}

        [Fact()]
        public void copyElementAfter_WithValidDocument_ShouldAddNewParagraphToDocument()
        {
			// arrange 

			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\Test.docx";
			var fileName = Path.GetFileName(pathToTemplate);
            var startingLineOfParagraphToCopy = "Name";

			Document document = new Document(pathToTemplate);

			// act

			document.CopyElementAfter(startingLineOfParagraphToCopy);

            // assert           

            using (var doc = WordprocessingDocument.Open(document.PathToDoc, false))
            {
                doc.MainDocumentPart.Should().NotBeNull();
                var paragraphToCopy = doc.MainDocumentPart.Document.Descendants<Paragraph>().
                    FirstOrDefault(p => p.InnerText.Contains(startingLineOfParagraphToCopy), null);

                paragraphToCopy.Should().NotBeNull();

                paragraphToCopy.ElementsAfter().First().InnerText.Should().Be(paragraphToCopy.InnerText);

                
            }

        }

	}
}