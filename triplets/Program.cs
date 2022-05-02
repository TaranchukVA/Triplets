using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace triplets
{
    /*
     *  Программа выполняет подсчёт триплетов в тексте и выводит десять самых частых.
     *  
     *  Для выбора файла с текстом необходимо ввести полный пусть и указать формат
     *  Пример: D:\Books\1984.txt
     *  
     *  Программа работает в поледовательном и параллельных режимах. Для выбора режима необходимо ввести 1 или 2
     *  1 - последовательный
     *  2 - параллельный
     *  
     *  Программа читает текст из файла и выполняет подсчёт триплетов.
     *  Чтение и подсчёт может занять некоторое время.
     *  Для файлов меньше 1МБ последовательная программа работает быстрее.
     *  
     *  В ответе выводятся десять самых часттых триплетов, их количество в файле, время работы программы и время выполнения подсчёта.
     *  
     */

    class Program
    {

        /// <summary>
        /// Процедура очищения от лишних знаков
        /// </summary>
        /// <param name="text">строка</param>
        /// <param name="symbols">набор знаков</param>
        public static void Replace(ref string text, string[] symbols)
        {
            foreach (var symbol in symbols)
                text = text.Replace(symbol, " ");
        }

        /// <summary>
        /// Очищение от лишних знаков
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string[] MarksFilter(string text)
        {
            string[] numbers = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string[] marks = { "\"", "@", "-", "!", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "№", ";", ":", "?", "=", "{", "}", "[", "]", "`", "~", "<", ">", "/", "\\", "|", ".", ",", "«", "»" };
            string[] specMarks = { "\n", "\r", "\t", "\r" };

            Replace(ref text, specMarks);

            return text.Split();
        }

        /// <summary>
        /// Очищение от коротких слов
        /// </summary>
        /// <param name="words"></param>
        public static void WordsFilter(ref string[] words)
        {
            List<string> wordList = new List<string>();

            foreach (var word in words)
                if (word.Length > 2) wordList.Add(word);

            words = wordList.ToArray();
        }

        public static SortedDictionary<string, uint> DWordsFilter(ref string[] words)
        {
            //List<string> wordList = new List<string>();

            var wordList = new Dictionary<string, uint>();

            foreach (var word in words)
                if (word.Length > 2)
                {
                    if (!wordList.TryAdd(word, 1))
                        wordList[word] += 1;
                }

            return new SortedDictionary<string, uint>(wordList);
        }

        /// <summary>
        /// Получить все триплеты
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string[] GetTriplets(string word)
        {
            List<string> subWords = new List<string>();

            for (int i = 0; i < word.Length - 2; i++)
                subWords.Add(word.Substring(i, 3));

            return subWords.ToArray();

        }

        static void Main(string[] args)
        {
            string repeat = "y";
            Console.WriteLine("Добро пожаловать в программу подсчёта триплетов.");
            do
            {
                Stopwatch totalTime = new Stopwatch();
                Stopwatch countingTime = new Stopwatch();


                Console.WriteLine("Введите путь к файлу");
                string path = $"{Console.ReadLine()}";
                if (String.IsNullOrEmpty(path)) { Console.WriteLine("Пустая строка. Попробуйте ещё раз."); continue; }
                if (!File.Exists(path)) { Console.WriteLine("Нет файла по заданному пути. Попробуйте ещё раз."); continue; }

                Console.WriteLine("Выберете режим работы:\n1\tпоследовательный,\n2\tмногопоточный.");
                int mode;
                while (!(int.TryParse(Console.ReadLine(), out mode) && (mode == 1 || mode == 2)))
                    Console.WriteLine("Неправильный выбор. Попробуйте ещё раз.");


                Console.WriteLine("Чтение и подготовка файла.");
                totalTime.Start();
                string text = File.ReadAllText(path);

                string[] words = MarksFilter(text);

                Console.WriteLine("Подсчёт триплетов начался.");
                countingTime.Start();

                var wordList = DWordsFilter(ref words);

                var data = new ConcurrentDictionary<string, uint>();

                if (mode == 1)
                {
                    foreach (var word in wordList)
                    {
                        foreach (var key in GetTriplets(word.Key))
                            data.AddOrUpdate(key, word.Value, (key, old) => old + word.Value);
                    };
                }
                else
                {
                    Parallel.ForEach(wordList, word =>
                    {
                        foreach (var key in GetTriplets(word.Key))
                            data.AddOrUpdate(key, word.Value, (key, old) => old + word.Value);
                    }
                        );
                }

                uint[] vals = data.Values.ToArray();
                string[] keys = data.Keys.ToArray();
                Array.Sort(vals, keys);

                totalTime.Stop();
                countingTime.Stop();

                var start = vals.Length - 1;
                var stop = (vals.Length > 10) ? vals.Length - 10 : 0;

                Console.WriteLine("\n\nРезультаты.");
                Console.WriteLine("Самые частые триплеты:");

                for (var i = start; i >= stop; i--)
                    Console.WriteLine($"{keys[i]} : {vals[i]}");

                Console.WriteLine($"Общее время работы: {totalTime.ElapsedMilliseconds} миллисек.");
                Console.WriteLine($"Время подсчёта триплетов: {countingTime.ElapsedMilliseconds} миллисек.");


                Console.WriteLine("\n\nХотите повторить? y - Да.");
                repeat = Console.ReadLine();
            } while (repeat == "y" || repeat == "у");
            Console.WriteLine("Для завершения нажмите любую клавишу.");
            Console.ReadKey();
        }
    }
}
