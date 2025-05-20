using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Olymp
{
    internal class Solution
    {
        static void Main(string[] args)
        {
            int n = Convert.ToInt32(Console.ReadLine());
            string str = Console.ReadLine();
            int answ = 0;
            foreach (char c in str)
            {
                string str1 = "";
                if (c == 'R')
                {
                    answ += 2;
                    continue;
                }
                else
                {
                    answ ++;
                    str1 = str.Replace('R', 'X').Replace('G', 'R').Replace('X', 'G');
                    str = str1;
                    continue;
                }
            }
            Console.WriteLine(answ);
        }
    }
}
