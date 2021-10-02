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

                foreach (var obj in result[Range.StartAt(Convert.ToInt32(result.Length*0.25+1))])
                {
                    obj.text = processMessage(obj.text);
                    obj.label = obj.label.ToLower();

                    switch (obj.label)
                    {
                        case "fake":
                                fakeArticles.Add(obj);
                                break;
                        case "real":
                                realArticles.Add(obj);
                                break;
                        default:
                                outherArticles.Add(obj);
                                break;
                    }
                }

                Console.WriteLine("Informações do documento: \nReais: {0}\nFalsos: {1}\nOutros: {2}\n-----------------", realArticles.Count, fakeArticles.Count, outherArticles.Count);

                test(fakeArticles,realArticles,details:true);


            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }
            
        }

        private static void test(List<csvObject> fakeArticles, List<csvObject> realArticles, int s=1, double i=0.5, bool details=false){
            int fakeCount = 0;
            int realCount = 0;

            /*
                Quero saber se o que existe no fakeArticles é falso
                ou seja se é falso ele é verdadeiramente positivo

                TP == O que está separado na tabela de "fake" é realmente uma noticia falsa.
                TN == O que está separado na tabela de "real" é realmente uma noticia verdadeira.
                FP == O que está separado na tabela de "fake" é realmente uma noticia verdadeira.
                FN == O que está separado na tabela de "real" é realmente uma noticia falsa.
            
            */

            foreach(var item in fakeArticles){
                if(fake(item.text,fakeArticles,realArticles,porcent:false)){
                    fakeCount++;
                }else{
                    realCount++;
                }
            }

            int truePositive = fakeCount;
            int falsePositive = realCount;

            fakeCount = 0;
            realCount = 0;

            foreach(var item in realArticles){
                if(fake(item.text,fakeArticles,realArticles,porcent:false)){
                    fakeCount++;
                }else{
                    realCount++;
                }
            }

            int falseNegative = fakeCount;
            int trueNegative = realCount;

            double fakePrecision = ( (double) truePositive / (double) (truePositive+falsePositive));
            double fakeRecall = ( (double) truePositive/ (double) (truePositive+falseNegative));
            double fakeScore = 2*(fakePrecision * fakeRecall)/(fakePrecision + fakeRecall);

            double realPrecision = ( (double) trueNegative / (double) (trueNegative+falseNegative));
            double realRecall =  ((double) trueNegative/ (double) (trueNegative+falsePositive));
            double realScore = 2*(realPrecision * realRecall)/(realPrecision + realRecall);

            double balancedResult = (fakeRecall+realRecall)/2;

            if(details){
                Console.WriteLine("TruePosive: {0}\nFalseNegative: {1}\nTrueNegative: {2}\nFalsePositive: {3}\n-----------------------\nPrecisão Falsos: {4:0.00}%\nRecall Falsos: {5:0.00}%\nF1-Score Falsos: {6:0.00}%\n-----------------------\nPrecisão Reais: {7:0.00}%\nRecall Reais: {8:0.00}%\nF1-Score Reais: {9:0.00}%\n-----------------------\nAcurácia: {10:0.00}%",truePositive,falseNegative,trueNegative,falsePositive,fakePrecision*100,fakeRecall*100,fakeScore*100,realPrecision*100,realRecall*100,realScore*100,balancedResult*100);
            }
        }

        private static bool fake(string message,List<csvObject> fakeArticles, List<csvObject> realArticles,int s=1, double p=0.5, bool porcent=true){
            double n = 0;
            double spam_freq = 0;
            double normal_freq = 0;

            List<csvObject> allArticles = new List<csvObject>();
            allArticles.AddRange(realArticles);
            allArticles.AddRange(fakeArticles);
            Dictionary<string,int> allWords = wordCount(allArticles);
            Dictionary<string,int> fakeWords = wordCount(fakeArticles);
            Dictionary<string,int> realWords = wordCount(realArticles);
            

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
