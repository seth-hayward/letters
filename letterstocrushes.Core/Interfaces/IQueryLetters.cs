﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryLetters
    {
        void hideLetter(int lucky_id, string userip, string cookie_value, string user_name, bool is_user_mod);
        void unhideLetter(int lucky_id, string userip, string cookie_value, string user_name, bool is_user_mod);
        void facebookLetter(int lucky_id, int toFacebookUID, int fromFacebookUID, string userip, string cookie_value, string user_name, bool is_user_mod);
        Boolean editLetter(int letter_id, string new_letter, string userip, string cookie_value, string user_name, bool is_user_mod);
        Letter getLetter(int id);
        int getLetterCount();
        int getLetterCountHomePage();
        int getLetterCountMorePage();
        List<Letter> getLetters(int greater_than_level, int page, int _pagesize);
        Letter getLatestFrontPageLetter();
        Letter getLastLetterSent();
        Letter getLastLetterFromIP(string ip);
        Letter getLetterByTag(string guid);
        List<Letter> getPopularLetters(Letter latest_front_page);
        List<Letter> search(string terms);
        void AddLetter(Letter letter);
    }
}
