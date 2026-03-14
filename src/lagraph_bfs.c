#include <stdio.h>
#include <stdlib.h>
#include <time.h>

#include <suitesparse/LAGraph.h>
#include <suitesparse/LAGraphX.h>
#include <suitesparse/GraphBLAS.h>

double get_time_seconds() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return ts.tv_sec + ts.tv_nsec * 1e-9;
}

int main(int argc, char **argv) {
    if (argc < 2) {
        fprintf(stderr, "Usage: %s <matrix.mtx> [source_vertex]\n", argv[0]);
        return 1;
    }

    char *matrix_file = argv[1];
    int64_t source = (argc >= 3) ? atoll(argv[2]) : 0;

    LAGraph_Init(NULL);

    GrB_Matrix A = NULL;
    FILE *f = fopen(matrix_file, "r");
    if (!f) {
        fprintf(stderr, "Error: cannot open file %s\n", matrix_file);
        LAGraph_Finalize(NULL);
        return 1;
    }

    double t0 = get_time_seconds();
    LAGraph_MMRead(&A, f, NULL);
    fclose(f);
    double t1 = get_time_seconds();

    GrB_Index nrows, ncols, nvals;
    GrB_Matrix_nrows(&nrows, A);
    GrB_Matrix_ncols(&ncols, A);
    GrB_Matrix_nvals(&nvals, A);

    printf("Matrix: %s\n", matrix_file);
    printf("Rows: %zu, Cols: %zu, NNZ: %zu\n", nrows, ncols, nvals);
    printf("Load time: %.3f s\n", t1 - t0);

    LAGraph_Graph G = NULL;
    LAGraph_New(&G, &A, LAGraph_ADJACENCY_UNDIRECTED, NULL);

    GrB_Vector src_vec = NULL;
    GrB_Vector_new(&src_vec, GrB_INT64, nrows);
    GrB_Vector_setElement(src_vec, 1, source);

    GrB_Matrix level = NULL;
    GrB_Matrix parent = NULL;

    t0 = get_time_seconds();
    LAGraph_MultiSourceBFS(&level, &parent, G, src_vec, NULL);
    t1 = get_time_seconds();

    printf("BFS time: %.3f s\n", t1 - t0);

    GrB_Matrix_free(&level);
    GrB_Matrix_free(&parent);
    GrB_Vector_free(&src_vec);

    LAGraph_Delete(&G, NULL);
    GrB_Matrix_free(&A);

    LAGraph_Finalize(NULL);

    return 0;
}
