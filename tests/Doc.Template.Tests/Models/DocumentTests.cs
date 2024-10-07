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
        public async Task SetTemplateAsync_WithValidPathToFile_ShouldCreateNewDocumentInTempFolderAndSetTemplateDocPath()
        {
			// arrange
			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\Test.docx";
			Document document = new Document();

			// act

			await document.SetTemplateAsync(pathToTemplate);

            // assert

            document.PathToTemplate.Should().Be(pathToTemplate);            
            File.Exists(@$"{Directory.GetCurrentDirectory()}\Temp\{Path.GetFileName(pathToTemplate)}").Should().BeTrue();
            File.GetAttributes(@$"{Directory.GetCurrentDirectory()}\Temp\{Path.GetFileName(pathToTemplate)}").
                GetHashCode().
                Should().
                Be(File.GetAttributes(pathToTemplate).GetHashCode());
        }

        [Fact()]
        public async Task SetTemplateAsync_WithInvalidPathToFile_ShouldThrowFileNotFoundException()
        {
            // arrange

            var pathToTemplate = "./InvalidFile.docx";
            Document document = new Document();

            // act

            Func<Task> action = async () => {await document.SetTemplateAsync(pathToTemplate); };



            // assert
            await action.Should().ThrowAsync<FileNotFoundException>()
                .WithMessage($"No template found at this location: {pathToTemplate}");
        }


		[Fact]
		public async Task SetTemplateAsync_WithValidStream_ShouldCreateNewDocumentInTempFolderAndSetTemplateDocPath()
		{
			// Arrange
			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\Test.docx";
			using var sourceStream = new FileStream(pathToTemplate, FileMode.Open, FileAccess.Read);
			Document document = new Document();

			// Act
			await document.SetTemplateAsync(sourceStream);

			// Assert
			document.PathToDoc.Should().NotBeNullOrEmpty();
			File.Exists(document.PathToDoc).Should().BeTrue();
			File.GetAttributes(document.PathToDoc)
				.GetHashCode().Should()
				.Be(File.GetAttributes(pathToTemplate).GetHashCode());
		}

		[Fact]
		public async Task SetTemplateAsync_WithNullStream_ShouldThrowArgumentNullException()
		{
			// Arrange
			Document document = new Document();

			// Act
			Func<Task> action = async () => await document.SetTemplateAsync(stream: null);

			// Assert
			await action.Should().ThrowAsync<ArgumentNullException>()
				.WithMessage("Source stream cannot be null.*");
		}

		[Fact()]
        public async Task DocumentDestructor_WithValidPathToFile_ShouldDeleteCreatedDocumentInTemp()
        {
			// arrange
			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\DestTest.docx";

			// act
			using (Document doc = new Document())
            {                           
                await doc.SetTemplateAsync(pathToTemplate);
            }
			Thread.Sleep(1000);
			System.GC.Collect();


			// assert
			File.Exists(@$"{Directory.GetCurrentDirectory()}\Temp\{Path.GetFileName(pathToTemplate)}").Should().BeFalse();

		}

        [Fact()]

        public async Task FindAndReplace_WithValidPhrases_ShouldContainPhraseToReplaceThreeTimes()
        {
			// arrange 

			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\Test.docx";
			var fileName = Path.GetFileName(pathToTemplate);

			Document document = new Document();

            await document.SetTemplateAsync(pathToTemplate);

            var phraseToFind = "Name";
            var phraseToReplace = "Changed Text";
            var count = 0;

            // act
            document.FindAndReplace(phraseToFind, phraseToReplace);

			// assert
			document.PathToDoc.Should().NotBeNull();

			using (var doc = WordprocessingDocument.Open(document.PathToDoc!, false))
			{
				foreach (var textElem in doc.MainDocumentPart!.Document.Descendants<Text>())
				{
					if (textElem.Text.Contains(phraseToReplace))
					{
						count++;
					}
				}
			}
			count.Should().Be(3);

		}

        [Fact()]
        public async Task FindAndReplace_WithValidPhrasesOnlyFirst_ShouldContainPhraseToReplaceOnce()
        {
            // arrange

            var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\Test.docx";
            var fileName = Path.GetFileName(pathToTemplate);

            Document document = new Document();
            await document.SetTemplateAsync(pathToTemplate);

            var phraseToFind = "Name";
            var phraseToReplace = "Changed Text";
            var count = 0;

			

			// act

			document.FindAndReplace(phraseToFind, phraseToReplace, true);

			// assert 

			document.PathToDoc.Should().NotBeNull();

			using (var doc = WordprocessingDocument.Open(document.PathToDoc!, false))
			{
				foreach (var textElem in doc.MainDocumentPart!.Document.Descendants<Text>())
				{
					if (textElem.Text.Contains(phraseToReplace))
					{
						count++;
					}
				}
			}
            count.Should().Be(1);

		}

        [Fact()]
        public async Task copyElementAfter_WithValidDocument_ShouldAddNewParagraphToDocument()
        {
			// arrange 

			var pathToTemplate = $@"{Directory.GetCurrentDirectory()}\..\..\..\Resources\Test.docx";
			var fileName = Path.GetFileName(pathToTemplate);
            var startingLineOfParagraphToCopy = "Name";

			Document document = new Document();
            await document.SetTemplateAsync(pathToTemplate);

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