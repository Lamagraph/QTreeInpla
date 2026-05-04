#!/usr/bin/env python3
"""
Visualize the sparsity pattern of a matrix from a Matrix Market (.mtx) file.
Optionally apply a permutation vector (e.g., from a reordering algorithm).
"""

import argparse
import numpy as np
import matplotlib.pyplot as plt
from scipy.io import mmread
import sys

def main():
    parser = argparse.ArgumentParser(
        description='Plot sparsity pattern of a matrix from a .mtx file.'
    )
    parser.add_argument('input', help='Input .mtx file')
    parser.add_argument('output', help='Output image file (e.g., .png, .pdf)')
    parser.add_argument('--perm', help='Optional file containing permutation vector (one integer per line)')
    parser.add_argument('--perm2', help='Optional second permutation vector for columns (if different from rows)')
    parser.add_argument('--title', default='Sparsity Pattern', help='Plot title')
    parser.add_argument('--cmap', default='viridis', help='Colormap (e.g., viridis, binary, plasma)')
    parser.add_argument('--markersize', type=float, default=1.0, help='Marker size for scatter plot')
    parser.add_argument('--figsize', nargs=2, type=float, default=[8, 8], help='Figure size (width height)')
    parser.add_argument('--dpi', type=int, default=100, help='DPI for output image')
    parser.add_argument('--downsample', type=int, default=1,
                        help='Downsampling factor: plot every nth non-zero (for very large matrices)')
    args = parser.parse_args()

    print(f"Reading matrix from {args.input}...")
    try:
        A = mmread(args.input)
    except Exception as e:
        print(f"Error reading matrix: {e}")
        sys.exit(1)

    # Convert to COO format for easy (row, col) coordinates
    if not hasattr(A, 'tocoo'):
        print("Warning: Input is not a sparse matrix; converting to sparse.")
        from scipy.sparse import csr_matrix
        A = csr_matrix(A)
    A_coo = A.tocoo()
    rows, cols = A_coo.row, A_coo.col
    n = A.shape[0]
    m = A.shape[1]

    print(f"Matrix size: {n} x {m}, nonzeros: {len(rows)}")

    # Apply permutation(s) if provided
    if args.perm:
        print(f"Loading row permutation from {args.perm}...")
        perm_rows = np.loadtxt(args.perm, dtype=int)
        if perm_rows.ndim != 1 or len(perm_rows) != n:
            print(f"Error: Permutation vector must be 1D with length {n}")
            sys.exit(1)
        rows = perm_rows[rows]  # reorder rows according to permutation

        if args.perm2:
            print(f"Loading column permutation from {args.perm2}...")
            perm_cols = np.loadtxt(args.perm2, dtype=int)
            if perm_cols.ndim != 1 or len(perm_cols) != m:
                print(f"Error: Second permutation vector must be 1D with length {m}")
                sys.exit(1)
        else:
            # If only one permutation and matrix is square, apply it to both rows and columns
            if n == m:
                print("Matrix is square: applying same permutation to rows and columns.")
                perm_cols = perm_rows
            else:
                print("Matrix is not square and only row permutation given: columns remain unchanged.")
                perm_cols = np.arange(m)  # identity permutation
        cols = perm_cols[cols]

    # Downsample if requested
    if args.downsample > 1:
        indices = np.arange(0, len(rows), args.downsample)
        rows = rows[indices]
        cols = cols[indices]
        print(f"Downsampled to {len(rows)} points.")

    # Create plot
    plt.figure(figsize=args.figsize, dpi=args.dpi)
    plt.scatter(cols, rows, s=args.markersize, c='k', marker='.', cmap=args.cmap, edgecolors='none')
    plt.xlim(-0.5, m-0.5)
    plt.ylim(n-0.5, -0.5)  # invert y-axis so row 0 is at top (like matrix layout)
    plt.xlabel('Column index')
    plt.ylabel('Row index')
    plt.title(args.title)
    plt.tight_layout()

    # Save
    print(f"Saving plot to {args.output}...")
    plt.savefig(args.output, dpi=args.dpi)
    print("Done.")

if __name__ == '__main__':
    main()
