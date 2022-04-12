#include <assert.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>

#define n 4096

int a[n][n];

static long diff_in_ms(struct timespec t1, struct timespec t2)
{
    struct timespec diff;
    if (t2.tv_nsec-t1.tv_nsec < 0) {
        diff.tv_sec  = t2.tv_sec - t1.tv_sec - 1;
        diff.tv_nsec = t2.tv_nsec - t1.tv_nsec + 1000000000;
    } else {
        diff.tv_sec  = t2.tv_sec - t1.tv_sec;
        diff.tv_nsec = t2.tv_nsec - t1.tv_nsec;
    }
    return (diff.tv_sec * 1000.0 + diff.tv_nsec / 1000000.0);
}

int main()
{
    // randomizing our initial prob
	for (int row = 0; row < n; row++) {
		for (int col = 0; col < n; col++) {
			a[row][col] = rand() % 9 + 1;
		}
	}

	struct timespec start, finish;

    // starting out timer
	clock_gettime(CLOCK_MONOTONIC, &start);

    // running avg for each array
    double total_avg = 0.0;
    for (int row = 0; row < n; row++) {
        double local_avg = 0.0;
		for (int col = 0; col < n; col++) {
			local_avg =+ a[row][col];
		}
        local_avg = local_avg / n;
        total_avg += local_avg;
	}

    // getting final results
	clock_gettime(CLOCK_MONOTONIC, &finish);
	double elasped = diff_in_ms(finish, start);
    total_avg = total_avg / n;

	printf("The loop took: %fms and the avg is: %f\n", elasped, total_avg);
	return 0;
}
