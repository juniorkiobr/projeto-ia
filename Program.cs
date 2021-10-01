using Microsoft.VisualBasic.FileIO;
using FileHelpers;
using StopWord;
using Annytab.Stemmer;
using System;
using System.Collections.Generic;

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
                List<csvObject> realArticles = new List<csvObject>();
                List<csvObject> fakeArticles = new List<csvObject>();
                List<csvObject> outherArticles = new List<csvObject>();

                foreach (csvObject obj in result)
                {
                    obj.text = processMessage(obj.text);
                    // Console.WriteLine(obj.text.Length > 125 ? obj.text.Substring(0, 125) : obj.text);
                    obj.title = processMessage(obj.text);
                    obj.label = obj.label.ToLower();

                    if (obj.label == "fake")
                    {
                        fakeArticles.Add(obj);
                    }
                    else if (obj.label == "real")
                    {
                        realArticles.Add(obj);
                    }
                    else
                    {
                        outherArticles.Add(obj);
                    }
                }

                Console.WriteLine("Informações do documento: \nReais: {0}\nFalsos: {0}\nOutros: {2}", realArticles.Count, fakeArticles.Count, outherArticles.Count);
                // printDictionary(wordCount(realArticles),"real");
                // printDictionary(wordCount(fakeArticles),"fake");
                // printDictionary(wordCount(outherArticles),"outher");
                List<csvObject> allArticles = new List<csvObject>();
                allArticles.AddRange(realArticles);
                allArticles.AddRange(fakeArticles);
                Dictionary<string,int> allWords = wordCount(allArticles);
                Dictionary<string,int> fakeWords = wordCount(fakeArticles);
                Dictionary<string,int> realWords = wordCount(realArticles);

                foreach(var item in realArticles){
                    fake(item.text,realWords,fakeWords,allWords);
                }


            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }
            
        }


        private static bool fake(string message,Dictionary<string, int> realWords,Dictionary<string, int> fakeWords,Dictionary<string, int> allWords,int s=1, double p=0.5, bool porcent=true){
            double n = 0;
            double spam_freq = 0;
            double normal_freq = 0;

            foreach (string word in processMessage(message).Split(' '))
            {
                if(fakeWords.ContainsKey(word)){
                    spam_freq = (double) fakeWords[word] / (double) allWords[word];
                }

                if(realWords.ContainsKey(word)){
                    normal_freq = (double) realWords[word] / (double) allWords[word];
                }

                if((spam_freq + normal_freq) != 0 && allWords.ContainsKey(word)){
                    double analiseWord = spam_freq / (spam_freq + normal_freq);
                    double corrAnalisedWord = (s*p + allWords[word] * analiseWord) / (allWords[word] + s);
                    n += (Math.Log(1 - corrAnalisedWord) - Math.Log(corrAnalisedWord));
                }
            }

            double result = 1/(1 + Math.Exp(n));

            if(porcent){
                Console.WriteLine("A probabilidade de ser um artigo falso é de :. {0:0.00} %",( result*100));
            }else if(result> 0.5) {
                return true;
            }else{
                return false;
            }
            return false;
        }
        private static void printDictionary(Dictionary<string, int> dicionarioReal, string labelType)
        {
            Console.WriteLine("Quantidade de Palavras Usadas em Artigos {0}:.",labelType == "fake"? "Falsos": labelType == "real"? "Reais":"Outros");
            foreach (var item in dicionarioReal)
            {
                Console.WriteLine("{0} -- {1}", item.Key, item.Value);
            }
        }

        private static Dictionary<string, int> wordCount(List<csvObject> lista)
        {
            Dictionary<string, int> dicionario = new Dictionary<string, int>();

            foreach(csvObject obj in lista){
                foreach (var item in obj.text.Split(' '))
                {
                    if (dicionario.ContainsKey(item))
                    {
                        dicionario[item]++;
                    }
                    else
                    {
                        dicionario.Add(item, 1);
                    }
                }
            } 
            
            return dicionario;
        }

        private static string processMessage(string message)
        {
            string[] words = System.IO.File.ReadAllLines("./words.txt");
            Stemmer stemmer = new EnglishStemmer();

            string messageWithoutSW = message.ToLower().RemoveStopWords("en");
            string processedMessage = "";
            foreach (String item in messageWithoutSW.Split(' '))
            {
                if (item.Length > 1 && Array.IndexOf(words, item) > -1)
                {
                    processedMessage += stemmer.GetSteamWord(item) + " ";
                }
            }

            return processedMessage;
        }
    }
}
