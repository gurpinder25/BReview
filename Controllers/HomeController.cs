using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BReview.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Data.SQLite;
using Microsoft.AspNetCore.Http;

namespace BReview.Controllers
{
    public class HomeController : Controller
    {

        public List<string[]> DataArray = new List<string[]> { };

        public static string btitle;
        public static string bauthor;
        public IActionResult Index()
        {
            try
            {

                //Opening new sqlite connection

                SQLiteConnection connection = new SQLiteConnection("Data Source=books.db;Version=3;New=False;Compress=True;");
                connection.Open();
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE lib (Title varchar, Author varchar)";
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                //
            }
            return View();
        }

        public IActionResult Result()
        {
            /*
             * This method
             * triggers a request
             * to an API server
             * and gets all the information
             */

            string JSONresponse;
            string userEntry = Request.QueryString.Value;
            userEntry = userEntry.Split("=")[1];

            if (userEntry == null)
            {
                Response.Redirect("/Home/Result");
            }
            else
            {

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://openlibrary.org/search.json?q=" + userEntry);
                try
                {
                    //Recieving response from the API
                    WebResponse response = httpWebRequest.GetResponse();
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                        JSONresponse = reader.ReadToEnd();


                        dynamic data = JObject.Parse(JSONresponse);
                        try
                        {

                            //Parsing JSON

                            btitle = data.docs[0].title_suggest;
                            bauthor = data.docs[0].author_name[0];

                            ViewData["btitle"] = btitle;
                            ViewData["bauthor"] = bauthor;
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }


                catch (WebException ex)
                {
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(IFormCollection formCollection)
        {
            /*
             *Create method simply inserts a new
             * entry into the already created table
             */
            string mtitle = Request.Form["title"];
            mtitle = mtitle.Replace(' ', '+');
            string mauthor = Request.Form["author"];

            SQLiteConnection connection = new SQLiteConnection("Data Source=books.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO lib (Title, Author) VALUES (" + "'" + mtitle + "'," + "'" + mauthor + "'" + ")";
            command.ExecuteNonQuery();

            return View();

        }

        public IActionResult Read()
        {

            /*
             *This method runs the SELECT sqlite query
             *to read and display all the elements
             * in the database
             */

            SQLiteConnection connection = new SQLiteConnection("Data Source=books.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();


            command.CommandText = "SELECT * FROM lib";

            SQLiteDataReader sqliteReader = command.ExecuteReader();

            while (sqliteReader.Read())
            {


                String[] Info_Book = new string[2];


                Info_Book[0] = sqliteReader.GetString(0);
                Info_Book[1] = sqliteReader.GetString(1);

                DataArray.Add(Info_Book);
            }

            ViewData["DataArray"] = DataArray;

            return View();
        }

        [HttpPost]
        public IActionResult Update()
        {
            /*
             *This method updates an entry
             * using SQLite update query
             */
            string mtitle = Request.Form["title"];
            mtitle = mtitle.Replace(' ', '+');
            string mauthor = Request.Form["author"];

            SQLiteConnection connection = new SQLiteConnection("Data Source=books.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "UPDATE lib SET Author=" + "'" + mauthor + "'" + " WHERE Title = " + "'" + mtitle + "'";
            command.ExecuteNonQuery();

            return View();
        }

        [HttpPost]
        public ActionResult Delete(IFormCollection formCollection)
        {
            //For deleting any record

            string mtitle = Request.Form["title"];
            mtitle = mtitle.Replace(' ', '+');

            SQLiteConnection connection = new SQLiteConnection("Data Source=books.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();


            command.CommandText = "DELETE FROM lib WHERE Title = " + "'" + mtitle + "'";
            command.ExecuteNonQuery();
            return View();

        }

    }
}
