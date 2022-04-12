#include <stdio.h>
#include <time.h>

#include "time-utils.h"
#include "random-arrays.h"

// sums up all the arrays available
int sum_arrays(int **arrays, int size, int count){
    int total = 0;
    for(int i = 0; i < size; i++){
        for(int j = 0; j < count; j++){
            total += arrays[j][i];
        }
    }
    return total;
}

int main(int argc, char *argv[])
{
    int size = 20009000;
    int count = 8;
    int **arrays = generate_random_arrays(size, count);
    struct timespec start, finish;

    // starting out timer
	clock_gettime(CLOCK_MONOTONIC, &start);

    // running our sum
    int sum = sum_arrays(arrays, size, count);

    // getting final results
	clock_gettime(CLOCK_MONOTONIC, &finish);
	double elasped = diff_in_ms(start, finish);

    free(arrays);
	printf("The loop took: %fms and the sum is: %d\n", elasped, sum);
}