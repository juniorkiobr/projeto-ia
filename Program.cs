using Microsoft.VisualBasic.FileIO;
using FileHelpers;
using System;

namespace inteligencia_artificial
{
    [DelimitedRecord(",")]
    class csvObject
    {
        [FieldOptional]
        string author;

        [FieldOptional]
        string timestamp;

        [FieldOptional]
        string title;

        [FieldOptional]
        string language;
        
        [FieldOptional]
        string type;
        
        [FieldOptional]
        string label;
        
        [FieldOptional]
        string withoutStopWords;
        
        [FieldOptional]
        string hasImage;
        
        [FieldOptional]
        string algo;
        [FieldOptional]
        string algo1;

        [FieldOptional]
        string algo2;

        

        public csvObject()
        {

        }
        public csvObject(string author, string timestamp, string title, string language, string type, string label, string withoutStopWords, string hasImage)
        {
            this.author = author;
            this.timestamp = timestamp;
            this.title = title;
            this.language = language;
            this.type = type;
            this.label = label;
            this.withoutStopWords = withoutStopWords;
            this.hasImage = hasImage;
        }

        override
        public string ToString()
        {
            return String.Format("{0} - {1} - {2} - {3}",this.author,this.timestamp,this.title,this.hasImage == "1" ? "Têm imagem" : "Não têm imagem");
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

                foreach (csvObject obj in result)
                {
                    Console.WriteLine("Informações do Artigo:. ");
                    Console.Write(obj+"\n");
                }
            } catch(Exception error)
            {
                Console.WriteLine(error);
            }
            
        }
    }
}
