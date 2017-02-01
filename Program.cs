using System;
using System.Collections.Generic;

namespace ConsoleApplication{
    public class Program{
        public static void Main(string[] args){
            string text = "I like eating ice cream. I also like drinking tea.";
            Console.WriteLine(text);
        }
        public void readText(string text){
            
        }
    }

    public class nGram{
        public string gram;
        public List<charNum> probabilities;
        public nGram(string gram){
           this.gram = gram;
           this.probabilities = new List<charNum>();
        }
        //TODO: Overloads
    }

    public class charNum{
        public char character;
        public int number;
        public charNum(char character, int number = 1){
           this.character = character;
           this.number = number;
        }
    }
}
