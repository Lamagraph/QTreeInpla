# QTreeInpla

QuadTree linear algebra implementation in Inpla.

# How to run experiments

* Make threaded patched Inpla:
```sh
git clone https://github.com/Lamagraph/inpla.git -b experiments

# Compile single-threaded version first (bug in vanilla Inpla):

make -C inpla && make -C inpla clean && make -C inpla thread
```

* Run `./scripts/run_experiments.sh $PATH_TO_INPLA $MAX_THREADS` to run all experiments in `./experiments/` directory and collect the results OR run the experiments yourself: `./inpla/inpla -f ./experiments/bcspwr10.in -t 4 > ./experiments/my_4threaded_result.txt`

* After `./scripts/run_experiments.sh` you can `./scripts/results_to_data.fsx` to extract data on bfs time and conversion time ready to be plotted

* Download mtx matrices from SuiteSparse matrix collection and convert them to experiments using `./scripts/mtx_to_experiment.fsx $PATH_TO_MTX_MATRIX`

# How to run golden tests

Ensure dotnet is installed.

1. Clone patched Inpla repository at [Lamagraph/inpla](https://github.com/Lamagraph/inpla)
2. Compile to obtain Inpla executable
3. Make sure you are in the project directory (Inpla's `use` directives are relative to current working directory)
4. `dotnet fsi test.fsx -- $PATH_TO_INPLA_EXECUTABLE` or simply `./test.fsx $PATH_TO_INPLA_EXECUTABLE`
