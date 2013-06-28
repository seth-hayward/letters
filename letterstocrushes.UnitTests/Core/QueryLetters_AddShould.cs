using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Services;
using letterstocrushes.Core.Interfaces;

namespace letterstocrushes.UnitTests.Core
{
    [TestClass]
    public class QueryLetters_AddShould
    {
        [TestMethod]
        public void AddLetter()
        {

            // Arrange
            Mock<IQueryLetters> mockQueryLetters = new Mock<IQueryLetters>();
            Mock<MailService> mockMailService = new Mock<MailService>();

            Mock<IQueryBookmarks> mockQueryBookmarks = new Mock<IQueryBookmarks>();

            BookmarkService mockBookmarkService = new BookmarkService(mockQueryBookmarks.Object);

            LetterService myLetterService = new LetterService(mockQueryLetters.Object, mockMailService.Object, mockBookmarkService);

            // Part of arrange -- set up the expectation that the AddLetter 
            // method will be called.
            mockQueryLetters.Setup(x => x.AddLetter(It.IsAny<Letter>()));

            // Act
            myLetterService.AddLetter(new Letter());

            // Assert
            mockQueryLetters.VerifyAll();
        }

    }
}
