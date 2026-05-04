import numpy as np
import scipy.sparse as sp
import scipy.io
import sys
import argparse
import warnings
import os

def get_output(output, input):
    if output == None:
        name, ext = os.path.splitext(input)
        return name + '_reordered' + ext
    else:
        return output

def reorder_rcm(A):
    """
    Переупорядочивание с помощью обратного алгоритма Катхилла-Макки (RCM).
    Возвращает перестановку для строк и столбцов (одна и та же).
    """
    # Симметризуем структуру ненулевых элементов
    A_bin = (A != 0).astype(int)
    A_sym = (A_bin + A_bin.T) > 0
    A_sym = A_sym.tocsr()
    A_sym.setdiag(0)           # убираем петли (диагональ)
    A_sym.eliminate_zeros()

    # Вычисляем перестановку RCM
    perm = sp.csgraph.reverse_cuthill_mckee(A_sym, symmetric_mode=True)
    return perm

def reorder_spectral(A, n_clusters=10):
    """
    Переупорядочивание с помощью спектральной кластеризации.
    Возвращает перестановку, упорядочивающую вершины по кластерам.
    Требует scikit-learn.
    """
    try:
        from sklearn.cluster import SpectralClustering
    except ImportError:
        raise ImportError("Метод 'spectral' требует scikit-learn. Установите: pip install scikit-learn")

    # Строим симметричный граф
    A_bin = (A != 0).astype(int)
    A_sym = (A_bin + A_bin.T) > 0
    A_sym = A_sym.tocsr()
    A_sym.setdiag(0)
    A_sym.eliminate_zeros()

    # Спектральная кластеризация на предвычисленной матрице смежности
    clustering = SpectralClustering(n_clusters=n_clusters,
                                    affinity='precomputed',
                                    random_state=0,
                                    assign_labels='kmeans')
    labels = clustering.fit_predict(A_sym)

    # Сортируем вершины по кластерам, внутри кластера – по исходному индексу
    unique_labels = np.unique(labels)
    perm = []
    for lab in unique_labels:
        indices = np.where(labels == lab)[0]
        # Дополнительно можно упорядочить внутри кластера, например по степени
        perm.extend(sorted(indices))  # сортировка по индексу сохраняет локальность
    return np.array(perm)

def main():
    parser = argparse.ArgumentParser(
        description='Переупорядочивание разреженной матрицы из файла Matrix Market и сохранение результата.'
    )
    parser.add_argument('input', help='Входной файл .mtx')
    parser.add_argument('--output', help='Выходной файл .mtx для переупорядоченной матрицы', required=False)
    parser.add_argument('--method', choices=['rcm', 'spectral'], default='rcm',
                        help='Метод переупорядочивания: rcm (обратный Катхилла-Макки) или spectral (спектральная кластеризация)')
    parser.add_argument('--perm-output', help='Опциональный файл для сохранения вектора перестановки (текст или .npy)')
    parser.add_argument('--n-clusters', type=int, default=10,
                        help='Количество кластеров для спектрального метода (по умолчанию 10)')

    args = parser.parse_args()

    # Чтение матрицы
    print(f"Чтение матрицы из {args.input}...")
    try:
        A = scipy.io.mmread(args.input)
    except Exception as e:
        print(f"Ошибка чтения: {e}")
        sys.exit(1)

    # Преобразование в разреженный формат (если необходимо)
    if not sp.issparse(A):
        print("Матрица плотная, преобразуем в разреженную (CSR).")
        A = sp.csr_matrix(A)
    else:
        A = A.tocsr()

    print(f"Размер матрицы: {A.shape}")
    if A.shape[0] != A.shape[1]:
        warnings.warn("Матрица не квадратная. Переупорядочивание будет выполнено только для строк (единая перестановка).")

    # Выбор и запуск метода
    print(f"Переупорядочивание методом: {args.method}")
    if args.method == 'rcm':
        perm = reorder_rcm(A)
    else:
        perm = reorder_spectral(A, n_clusters=args.n_clusters)

    print(f"Длина вектора перестановки: {len(perm)}")

    # Применение перестановки
    if A.shape[0] == A.shape[1]:
        A_reordered = A[perm, :][:, perm]
    else:
        # Для прямоугольной матрицы применяем перестановку только к строкам
        A_reordered = A[perm, :]

    # Сохранение результата
    output = get_output(args.output, args.input)
    print(f"Запись переупорядоченной матрицы в {output}...")
    try:
        scipy.io.mmwrite(output, A_reordered,
                         comment=f'Reordered with method={args.method}')
    except Exception as e:
        print(f"Ошибка записи: {e}")
        sys.exit(1)

    # Сохранение вектора перестановки (опционально)
    if args.perm_output:
        if args.perm_output.endswith('.npy'):
            np.save(args.perm_output, perm)
            print(f"Перестановка сохранена в формате .npy: {args.perm_output}")
        else:
            np.savetxt(args.perm_output, perm, fmt='%d')
            print(f"Перестановка сохранена в текстовом формате: {args.perm_output}")

    print("Готово.")

if __name__ == '__main__':
    main()
