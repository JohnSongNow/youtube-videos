CC = gcc
CFLAGS += -std=gnu11 -Wall -Werror -g3 -DNDEBUG 
EXTRA_CFLAGS += -O3 -fopenmp
LDFLAGS += -lm -fopenmp 

all: avg-run-test array-sum array-sum-parallel

avg-run-test: avg-run-test.o
	$(CC) $^ -o $@ $(LDFLAGS)

#avg-run-test-parallel: avg-run-test-parallel.o
	#$(CC) $^ -o $@ $(LDFLAGS) $(EXTRA_CFLAGS)

array-sum: array-sum.o
	$(CC) time-utils.h random-arrays.c $^ -o $@ $(LDFLAGS)

array-sum-parallel: array-sum-parallel.o
	$(CC) time-utils.h random-arrays.c $^ -o $@ $(LDFLAGS)

%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

clean:
	rm -f *.o avg-run-test array-sum array-sum-parallel