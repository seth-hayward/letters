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

            Mock<BookmarkService> mockBookmarkService = new Mock<BookmarkService>();
            Mock<BlockService> mockBlockService = new Mock<BlockService>();

            LetterService myLetterService = new LetterService(mockQueryLetters.Object, mockMailService.Object, mockBookmarkService.Object, mockBlockService.Object);

            mockQueryLetters.Setup(x => x.editLetter(It.IsAny<int>(), It.IsAny<String>(),It.IsAny<String>(),It.IsAny<String>(),It.IsAny<String>(),It.IsAny<bool>())).Returns(true);

            // Act
            bool result = myLetterService.editLetter(1, string.Empty, string.Empty, string.Empty, string.Empty, true);

            // Assert
            mockQueryLetters.VerifyAll();

        }
    }
}
