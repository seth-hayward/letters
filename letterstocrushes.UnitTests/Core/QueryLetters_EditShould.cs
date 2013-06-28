using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Services;
using letterstocrushes.Core.Interfaces;

namespace letterstocrushes.UnitTests.Core
{
    [TestClass]
    public class QueryLetters_EditShould
    {
        [TestMethod]
        public void EditLetter()
        {

            // Arrange
            Mock<IQueryLetters> mockQueryLetters = new Mock<IQueryLetters>();
            Mock<MailService> mockMailService = new Mock<MailService>();

            Mock<IQueryBookmarks> mockQueryBookmarks = new Mock<IQueryBookmarks>();

            BookmarkService mockBookmarkService = new BookmarkService(mockQueryBookmarks.Object);

            LetterService myLetterService = new LetterService(mockQueryLetters.Object, mockMailService.Object, mockBookmarkService);

            mockQueryLetters.Setup(x => x.editLetter(It.IsAny<int>(), It.IsAny<String>(),It.IsAny<String>(),It.IsAny<String>(),It.IsAny<String>(),It.IsAny<bool>())).Returns(true);

            // Act
            bool result = myLetterService.editLetter(1, string.Empty, string.Empty, string.Empty, string.Empty, true);

            // Assert
            mockQueryLetters.VerifyAll();

        }
    }
}
