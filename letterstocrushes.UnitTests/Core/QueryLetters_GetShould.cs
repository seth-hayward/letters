using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Services;
using letterstocrushes.Core.Interfaces;
using Moq;

namespace letterstocrushes.UnitTests.Core
{
    [TestClass]
    public class QueryLetters_GetShould
    {
        [TestMethod]
        public void GetLetter()
        {

            // Arrange
            Mock<IQueryLetters> mockQueryLetters = new Mock<IQueryLetters>();
            Mock<MailService> mockMailService = new Mock<MailService>();

            Mock<IQueryBookmarks> mockQueryBookmarks = new Mock<IQueryBookmarks>();

            Mock<BookmarkService> mockBookmarkService = new Mock<BookmarkService>();
            Mock<BlockService> mockBlockService = new Mock<BlockService>();

            LetterService myLetterService = new LetterService(mockQueryLetters.Object, mockMailService.Object, mockBookmarkService.Object, mockBlockService.Object);

            // Part of arrange -- set up the expectation that the AddLetter 
            // method will be called.
            mockQueryLetters.Setup(x => x.getLetter(It.IsAny<int>()));

            // Act
            myLetterService.getLetter(10000);

            // Assert
            mockQueryLetters.VerifyAll();
        }

    }
}
