#!/bin/bash

if [ "$#" -ne 3 ]; then
  echo "Usage: $0 <max_threads> <matrices_dir> <lagraph_bfs_path>"
  echo "  max_threads: maximum number of threads to test (1 to N)"
  echo "  matrices_dir: directory containing .mtx files"
  echo "  lagraph_bfs_path: path to lagraph_bfs executable"
  exit 1
fi

MAX_THREADS="$1"
MATRICES_DIR="$2"
LABRAPH_BIN="$3"

if [ ! -d "$MATRICES_DIR" ]; then
  echo "Error: matrices directory '$MATRICES_DIR' not found"
  exit 1
fi

RESULTS_DIR="./experiments/results_lagraph"
mkdir -p "$RESULTS_DIR"

if [ ! -x "$LABRAPH_BIN" ]; then
  echo "Error: lagraph_bfs executable not found at $LABRAPH_BIN"
  exit 1
fi

for MTX_FILE in "$MATRICES_DIR"/*.mtx; do
  [ -e "$MTX_FILE" ] || continue

  FILENAME=$(basename "$MTX_FILE")
  MATRIX_NAME="${FILENAME%.mtx}"

  RESULTS_SUBDIR="$RESULTS_DIR/${MATRIX_NAME}_lagraph"
  mkdir -p "$RESULTS_SUBDIR"

  echo "Starting experiment: $MATRIX_NAME"

  THREADS=1
  while [ "$THREADS" -le "$MAX_THREADS" ]; do
    echo "  -> Running with $THREADS thread(s)"
    
    OUTPUT_FILE="$RESULTS_SUBDIR/threads${THREADS}.output"
    
    {
      echo "LAGraph $MATRIX_NAME"
      echo "(load matrix, $THREADS threads, 0.00 sec)"
      OMP_NUM_THREADS=$THREADS "$LABRAPH_BIN" "$MTX_FILE" 0
    } > "$OUTPUT_FILE" 2>&1
    
    if [ $THREADS -eq 1 ]; then
      THREADS=2
    else
      THREADS=$((THREADS + 2))
    fi
  done
done

echo "All experiments finished successfully"
