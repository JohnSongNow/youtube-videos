#include <stdlib.h>

// returns an array with size random numbers
int* generate_random_array(int size){
    int *numbers = malloc(sizeof(int) * size);
    
    for(int i = 0; i < size; i++){
        numbers[i] = rand() % 80;
    }

    return numbers;
}

// returns count amount of arrays each with size random numbers
int **generate_random_arrays(int size, int count){

    int **arrays = malloc(sizeof(int) * size * count);
    for(int i = 0; i < count; i++){
        arrays[i] = generate_random_array(size);
    }

    return arrays;
}