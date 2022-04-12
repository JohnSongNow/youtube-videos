import random, time

n = 4096

# randomizing our initial problem
a = [[random.randint(1,10) for x in range(n)] for y in range(n)]

# starting out timer
start = time.time()

# running avg for each array
total_avg = 0.0
for row in a:
    local_avg = 0.0
    for col in row:
        local_avg += col
    local_avg = local_avg / n
    total_avg += local_avg

# getting final results
end = time.time()
elasped = end - start
total_avg = total_avg / n

# the total_avg is a debug function to check that the randomized values within range of expected
print("The loop took: " + str(elasped) + ' ms and the avg is: ' + str(total_avg))