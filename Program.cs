﻿using Microsoft.VisualBasic.FileIO;
using FileHelpers;
using StopWord;
using Annytab.Stemmer;
using System;

namespace inteligencia_artificial
{
    [DelimitedRecord(",")]
    [IgnoreEmptyLines()]
    [IgnoreFirst()]
    class csvObject
    {
        /*
                Para a criação da tabela só é necessário title, text, label
        */

        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string title;
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string text;
        public string label;
        

        public csvObject(){
        

        }

        override
        public string ToString()
        {
            return String.Format("{0} - {1} - {2}",
                this.title.Length > 10? this.title.Substring(0,10) : this.title,
                this.text.Length > 10? this.text.Substring(0,10) : this.text,this.label);
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var engine = new FileHelperEngine<csvObject>();
                var result = engine.ReadFile("./news_articles.csv");
                string[] words =  System.IO.File.ReadAllLines("./words.txt");
                int realArticles = 0;
                int fakeArticles = 0;
                int outherArticles = 0;
                Stemmer stemmer = new EnglishStemmer();

                foreach (csvObject obj in result)
                {
                    var tmp = obj.text.ToLower().RemoveStopWords("en");
                    string tmp1 = "";
                    foreach (String item in tmp.Split(' '))
                    {
                        if(item.Length > 1 && Array.IndexOf(words, item) > -1 ){
                            tmp1 += stemmer.GetSteamWord(item)+" ";
                        }
                    }
                    Console.WriteLine(tmp1.Length > 125? tmp1.Substring(0,125): tmp1);
                    obj.text = tmp1;
                    tmp = obj.title.ToLower().RemoveStopWords("en");
                    obj.title = tmp;
                    obj.label = obj.label.ToLower();


                    if(obj.label == "fake"){
                        fakeArticles++;
                    }else if(obj.label == "real"){
                        realArticles++;
                    }else{
                        outherArticles++;
                    }
                }

                Console.WriteLine("Informações do documento: \nReais: {0}\nFalsos: {0}\nOutros: {2}",realArticles,fakeArticles,outherArticles);
            } catch(Exception error)
            {
                Console.WriteLine(error);
            }
            
        }
    }
}
