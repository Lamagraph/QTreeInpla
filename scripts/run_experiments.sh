#!/bin/bash

if [ "$#" -ne 3 ]; then
  echo "Usage: $0 <path_to_inpla> <max_threads> (bfs | tc)"
  exit 1
fi

INPLA_PATH="$1"
MAX_THREADS="$2"
ALGORITHM="$3"

case "$3" in
    "bfs"|"tc")
        ;;
    *)
        echo "Error: Third argument must be 'bfs' or 'tc'."
        exit 1
        ;;
esac

EXPERIMENTS_FOLDER="./experiments_${ALGORITHM}"

if [ ! -d $EXPERIMENTS_FOLDER ]; then
  echo "Error: ${EXPERIMENTS_FOLDER} directory not found"
  exit 1
fi

for EXPERIMENT in $EXPERIMENTS_FOLDER/*.in; do
  [ -e "$EXPERIMENT" ] || continue

  FILENAME=$(basename "$EXPERIMENT")
  EXPERIMENT_NAME="${FILENAME%.in}"

  RESULTS_DIR="$EXPERIMENTS_FOLDER/results/$EXPERIMENT_NAME"
  mkdir -p "$RESULTS_DIR"

  echo "Starting experiment: $EXPERIMENT_NAME"

  THREADS=1

  while [ "$THREADS" -le "$MAX_THREADS" ]; do
    echo "  -> Running with $THREADS thread(s)"
    "$INPLA_PATH" -f "$EXPERIMENT" -t "$THREADS" >"$RESULTS_DIR/threads${THREADS}.output"
    if [ $THREADS -eq 1 ]; then
      THREADS=2
    else
      THREADS=$((THREADS + 2))
    fi
  done
done

echo "All experiments finished successfully"
