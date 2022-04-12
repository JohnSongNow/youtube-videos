#include <stdio.h>
#include <time.h>
#include <pthread.h>

#include "time-utils.h"
#include "random-arrays.h"

struct array_sum_struct{
    int *array;
    int size;
    int sum;
};

void *sum_array(void *args){
    // mmust cast our args
    struct array_sum_struct *s = args;
    int *array = s->array;

    // compute the sum now
    for(int i = 0; i < s->size; i++){
        s->sum += array[i];
    }

    // exiting our thread
	pthread_exit((void*)0);
}

// sums up all the arrays availableg
int sum_arrays(int **arrays, int size, int count){
    int total = 0;
	pthread_t threads[count];
    struct array_sum_struct array_sum_structs[count];
	int rc;
	int rj;

    // create 1 thread for each different array
	for (int i = 0; i < count; i++) {
        array_sum_structs[i].array = arrays[i];
        array_sum_structs[i].size = size;
		rc = pthread_create(&threads[i], NULL, sum_array, &(array_sum_structs[i]));
		if(rc){
			printf("Error; return code from pthread_create is %d\n", rc);
			return total;
		}
	}

	// wait for therads to join and finish before preceeding
	for(int i = 0; i < count; i++){
		rj = pthread_join(threads[i], NULL);
		if(rj){
			printf("Error; return code from pthraed_join is %d\n", rj);
			return total;
		}
        
        // sum up the actual array now
        total += array_sum_structs[i].sum;
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