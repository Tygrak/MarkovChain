using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SharpParser;

namespace ConsoleApplication{
    public class Program{
        public static void Main(string[] args){
            /*string path = Directory.GetCurrentDirectory()+"\\markov_input.txt";
            var inputText = File.ReadAllLines(path);
            string text = string.Join(" ", inputText);*/
            List<string> links = new List<string>(readLinks());
            string text = markovText.readText();
            string url = "https://www.explainxkcd.com/wiki/index.php/";
            for (int i = 1; i <= 100; i++){
                string nextUrl = url + i.ToString();
                if(links.Contains(nextUrl)){
                    continue;
                } else{
                    links.Add(nextUrl);
                }
                Console.WriteLine("Currently parsing: "+nextUrl);
                page pag = new page(nextUrl);
                int pos = pag.html.IndexOf("Trivia");
                pag = new page(url, pag.html.Split(new string[]{"<span class=\"mw-headline\" id=\"Trivia\">Trivia</span>"}, StringSplitOptions.None)[0]);
                section[] transcript = pag.findAllSections("dl");
                for (int j = 0; j < transcript.Length; j++){
                    text += transcript[j].content;
                }
            }
            saveLinks(links.ToArray());
            markovText.saveText(text);
            markovText mText = new markovText(2, text, true);
            Console.WriteLine(mText.generateText(100));
        }

        public static void saveLinks(string[] links){
            string path = Directory.GetCurrentDirectory()+"\\links.txt";
            File.WriteAllLines(path, links);
        }

        public static string[] readLinks(){
            string path = Directory.GetCurrentDirectory()+"\\links.txt";
            return File.ReadAllLines(path);
        }
    }

    public class markovText{
        public int size;
        public bool usesWords = false;
        public string inputText;
        public List<ngram> ngrams = new List<ngram>();

        public markovText(int size, string inputText, bool usesWords = false){
            this.size = size;
            this.inputText = inputText;
            this.usesWords = usesWords;
            createNgrams(size, inputText);
        }

        public static void saveText(string text){
            string path = Directory.GetCurrentDirectory()+"\\input.txt";
            File.WriteAllText(path, text);
        }

        public static string readText(){
            string path = Directory.GetCurrentDirectory()+"\\input.txt";
            return File.ReadAllText(path);
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
            inputText = inputText.Replace("„", "");
            inputText = inputText.Replace("“", "");
            inputText = inputText.Replace("\n", "");
            List<string> gram = new List<string>();
            if(usesWords){
                string[] words = inputText.Split(new string[] {" "}, StringSplitOptions.None);
                for (int i = 0; i < words.Length-size; i++){
                    for (int j = 0; j < size; j++){
                        gram.Add(words[i+j]);
                    }
                    string nextString = words[i+size];
                    string[] gramA = gram.ToArray();
                    ngram foundGram = ngrams.Find(x => Enumerable.SequenceEqual(gramA, x.gram));
                    if(foundGram != null){
                        stringNum foundString = foundGram.nextStrings.Find(x => x.str.Equals(nextString));
                        if(foundString != null){
                            foundString.number += 1;
                        } else{
                            foundGram.nextStrings.Add(new stringNum(nextString));
                        }
                    } else{
                        ngrams.Add(new ngram(gramA, nextString));
                    }
                    gram.Clear();
                }
            } else{
                for (int i = 0; i < inputText.Length-size-1; i++){
                    string gramS = inputText.Substring(i, size);
                    string nextString = inputText.Substring(i+size,1);
                    for (int j = 0; j < gramS.Length; j++){
                        gram.Add(gramS[j].ToString());
                    }
                    string[] gramA = gram.ToArray();
                    ngram foundGram = ngrams.Find(x => Enumerable.SequenceEqual(gramA, x.gram));
                    if(foundGram != null){
                        stringNum foundString = foundGram.nextStrings.Find(x => x.str.Equals(nextString));
                        if(foundString != null){
                            foundString.number += 1;
                        } else{
                            foundGram.nextStrings.Add(new stringNum(nextString));
                        }
                    } else{
                        ngrams.Add(new ngram(gramA, nextString));
                    }
                    gram.Clear();
                }
            }
        }

        public string generateText(int length){
            string text = "";
            Random rand = new Random();
            int num = rand.Next(0,ngrams.Count-1);
            ngram currGram = ngrams[num];
            if(usesWords){
                text += string.Join(" ", currGram.gram);
                for (int i = 0; i < length-size; i++){
                    num = rand.Next(0,currGram.nextCount());
                    text += " " + currGram.nextGet(num);
                    string[] words = text.Split(new string[] {" "}, StringSplitOptions.None);
                    List<string> listGram = new List<string>();
                    for (int j = words.Length-size; j < words.Length; j++){
                        listGram.Add(words[j]);
                    }
                    string[] nextGram = listGram.ToArray();
                    currGram = ngrams.Find(x => Enumerable.SequenceEqual(nextGram, x.gram));
                    if(currGram == null){
                        num = rand.Next(0,ngrams.Count-1);
                        currGram = ngrams[num];
                    }
                }
            } else{
                text += string.Join("", currGram.gram);
                for (int i = 0; i < length-size; i++){
                    num = rand.Next(0,currGram.nextCount());
                    text += currGram.nextGet(num);
                    string stringGram = text.Substring(text.Length-size);
                    List<string> listGram = new List<string>();
                    for (int j = 0; j < stringGram.Length; j++){
                        listGram.Add(stringGram[j].ToString());
                    }
                    string[] nextGram = listGram.ToArray();
                    currGram = ngrams.Find(x => Enumerable.SequenceEqual(nextGram, x.gram));
                    if(currGram == null){
                        num = rand.Next(0,ngrams.Count-1);
                        currGram = ngrams[num];
                    }
                }
            }
            return text;
        }
    }

    public class ngram{
        public string[] gram;
        public List<stringNum> nextStrings;

        public ngram(string[] gram){
           this.gram = gram;
           this.nextStrings = new List<stringNum>();
        }
        public ngram(string[] gram, string nextString){
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
