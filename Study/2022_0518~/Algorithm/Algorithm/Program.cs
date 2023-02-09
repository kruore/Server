using System;
using System.Linq;
using System.Collections.Generic;
class program
{
    static void Main(string[] args)
    {
        var a = Console.ReadLine().Split(' ');
        //두개의 자연수가 주어진다.

        var data_a = int.Parse(a[0]);
        int[] dataSlot = new int[data_a];
        var data_b = int.Parse(a[1]);

        while(true)
        {
            for (int i = 0; i > data_a; i++)
            {

            }
        }
        
    }

    public void Combination(int a, int b)
    {
        int[] dataSlot = new int[a];
        var data_b = b;
        for (int i = 0; i < a;i++)
        {
            dataSlot[i] = i;
        }
        // 중복 없이 M 개를 고른다. b =m
        Choose(dataSlot,b);

    }
    public void Choose(int[] dataSlot, int b)
    {
        int count = 0;
        int[] list = new int[b];
        List<int> data =  new List<int>();
        while(b==count)
        {
            for(int i=0;i<b; i++)
            {
                list[i] = dataSlot[i];
            }
            count++;
        }
    }

}