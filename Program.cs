using System;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApplication{
    public class Program{
        public static void Main(string[] args){
            string path = Directory.GetCurrentDirectory()+"\\markov_input.txt";
            var inputText = File.ReadAllLines(path);
            string text = string.Join(" ", inputText);
            //Console.WriteLine(text);
            markovText mText = new markovText(3, text);
            Console.WriteLine(mText.generateText(420));
        }
    }

    public class markovText{
        public int size;
        public string inputText;
        public List<ngram> ngrams = new List<ngram>();
        public markovText(int size, string inputText){
            this.size = size;
            this.inputText = inputText;
            createNgrams(size, inputText);
        }

        public void printGrams(){
            for (int i = 0; i < ngrams.Count; i++){
                Console.WriteLine("Gram: "+ngrams[i].gram);
                for (int j = 0; j < ngrams[i].nextStrings.Count; j++){
                    Console.WriteLine(ngrams[i].nextStrings[j].str + " " + ngrams[i].nextStrings[j].number.ToString());
                }
            }
        }

        public void createNgrams(int size, string inputText){
            inputText = inputText.ToLower();
            for (int i = 0; i < inputText.Length-size-1; i++){
                string gram = inputText.Substring(i, size);
                string nextString = inputText.Substring(i+size,1);
                ngram foundGram = ngrams.Find(x => x.gram.Equals(gram));
                if(foundGram != null){
                    stringNum foundString = foundGram.nextStrings.Find(x => x.str.Equals(nextString));
                    if(foundString != null){
                        foundString.number += 1;
                    } else{
                        foundGram.nextStrings.Add(new stringNum(nextString));
                    }
                } else{
                    ngrams.Add(new ngram(gram, nextString));
                }
            }
        }

        public string generateText(int length){
            string text = "";
            Random rand = new Random();
            int num = rand.Next(0,ngrams.Count-1);
            ngram currGram = ngrams[num];
            text += currGram.gram;
            for (int i = 0; i < length-size; i++){
                num = rand.Next(0,currGram.nextCount());
                text += currGram.nextGet(num);
                string nextGram = text.Substring(text.Length-size);
                currGram = ngrams.Find(x => x.gram.Equals(nextGram));
                if(currGram == null){
                    Console.WriteLine(nextGram);
                    num = rand.Next(0,ngrams.Count-1);
                    currGram = ngrams[num];
                }
            }
            return text;
        }
    }

    public class ngram{
        public string gram;
        public List<stringNum> nextStrings;
        public ngram(string gram){
           this.gram = gram;
           this.nextStrings = new List<stringNum>();
        }
        public ngram(string gram, string nextString){
           this.gram = gram;
           this.nextStrings = new List<stringNum>();
           nextStrings.Add(new stringNum(nextString));
        }

        public int nextCount(){
            int count = 0;
            for (int i = 0; i < nextStrings.Count; i++){
                count += nextStrings[i].number;
            }
            return count;
        }

        public string nextGet(int num){
            int count = 0;
            for (int i = 0; i < nextStrings.Count; i++){
                count += nextStrings[i].number;
                if(count>num){
                    return nextStrings[i].str;
                }
            }
            throw new Exception("nextGet out of range exception.");
        }
    }

    public class stringNum{
        public string str;
        public int number;
        public stringNum(string str, int number = 1){
           this.str = str;
           this.number = number;
        }
    }
}
