﻿using System.Collections;

//Mathematics formula that are useful in other situation
public static class Utility
{

    //to randomizely generate obstacle in the map using "Fisher Yates Shuffle" method
    public static T[] ShuffleArray<T> (T[] array,int seed)
    {
        System.Random prng = new System.Random(seed);  //random generate a prgn (pseuduo random number generator) using seed

        /* The basic method given for generating a random permutation of the numbers 1 through N goes as follows:

        1.Write down the numbers from 1 through N.
        2.Pick a random number k between one and the number of unstruck numbers remaining (inclusive).
        3.Counting from the low end, strike out the kth number not yet struck out, and write it down at the end of a separate list.
        4.Repeat from step 2 until all the numbers have been struck out.
        5.The sequence of numbers written down in step 3 is now a random permutation of the original numbers.

        Provided that the random numbers picked in step 2 above are truly random and unbiased, so will the resulting permutation be. */

        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length); 
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array; //return the array of prng
    }
}