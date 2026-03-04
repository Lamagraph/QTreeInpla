#!/bin/bash

if [ "$#" -ne 2 ]; then
  echo "Usage: $0 <path_to_inpla> <max_threads>"
  exit 1
fi

INPLA_PATH="$1"
MAX_THREADS="$2"

if [ ! -d "./experiments" ]; then
  echo "Error: ./experiments/ directory not found"
  exit 1
fi

for EXPERIMENT in ./experiments/*.in; do
  [ -e "$EXPERIMENT" ] || continue

  FILENAME=$(basename "$EXPERIMENT")
  EXPERIMENT_NAME="${FILENAME%.in}"

  RESULTS_DIR="./experiments/results/$EXPERIMENT_NAME"
  mkdir -p "$RESULTS_DIR"

  echo "Starting experiment: $EXPERIMENT_NAME"

  THREADS=1

  while [ "$THREADS" -le "$MAX_THREADS" ]; do
    echo "  -> Running with $THREADS threads"
    "$INPLA_PATH" -f "$EXPERIMENT" -t "$THREADS" >"$RESULTS_DIR/threads${THREADS}.output"

    THREADS=$((THREADS * 2))
  done
done

echo "All experiments finished successfully"
