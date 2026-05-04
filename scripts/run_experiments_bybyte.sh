#!/bin/bash

if [ "$#" -ne 4 ]; then
  echo "Usage: $0 <path_to_inpla> <max_threads> <number_of_runs> (bfs | tc | sssp)"
  exit 1
fi

INPLA_PATH="$1"
MAX_THREADS="$2"
NUMBER_OF_RUNS="$3"
ALGORITHM="$4"

case "$4" in
"bfs" | "tc" | "sssp") ;;
*)
  echo "Error: Third argument must be 'bfs', 'tc' or 'sssp'."
  exit 1
  ;;
esac

EXPERIMENTS_FOLDER="./experiments_${ALGORITHM}"

if [ ! -d $EXPERIMENTS_FOLDER ]; then
  echo "Error: ${EXPERIMENTS_FOLDER} directory not found"
  exit 1
fi

RUN=1

# Associative memory array:

declare -A memory

memory[1]=1032444062

THREADS=2

while [ "$THREADS" -le "$MAX_THREADS" ]; do
  memory[$THREADS]=$((memory[1] / THREADS))
  THREADS=$((THREADS + 2))
done

while [ "$RUN" -le "$NUMBER_OF_RUNS" ]; do
  indent=""
  echo "${indent}Run $RUN out of $NUMBER_OF_RUNS"

  for EXPERIMENT in $EXPERIMENTS_FOLDER/*.in; do
    indent="  "
    [ -e "$EXPERIMENT" ] || continue

    FILENAME=$(basename "$EXPERIMENT")
    EXPERIMENT_NAME="${FILENAME%.in}"

    RESULTS_DIR="$EXPERIMENTS_FOLDER/results/$EXPERIMENT_NAME/run${RUN}"
    mkdir -p "$RESULTS_DIR"

    echo "${indent}Starting experiment: $EXPERIMENT_NAME"

    THREADS=1

    while [ "$THREADS" -le "$MAX_THREADS" ]; do
      indent="    "
      echo "${indent}-> Running with $THREADS thread(s)"

      # -Xms 22 -Xmt 0 for better control over memory
      # -Xms 23 or -Xms 24 might be even better

      "$INPLA_PATH" -f "$EXPERIMENT" -t "$THREADS" -Xms "${memory[$THREADS]}" >"$RESULTS_DIR/threads${THREADS}.output"
      if [ $THREADS -eq 1 ]; then
        THREADS=2
      else
        THREADS=$((THREADS + 2))
      fi
    done
  done
  RUN=$((RUN + 1))
done

echo "All experiments finished successfully"
