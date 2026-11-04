# MNIST in CSV

## About Dataset

# The MNIST dataset provided in a easy-to-use CSV format

The [original dataset (opens in a new tab)](http://yann.lecun.com/exdb/mnist/) is in a format that is difficult for beginners to use. This dataset uses the work of [Joseph Redmon (opens in a new tab)](https://pjreddie.com/) to provide the [MNIST dataset in a CSV format (opens in a new tab)](https://pjreddie.com/projects/mnist-in-csv/).

The dataset consists of two files:

1. `mnist_train.csv`
2. `mnist_test.csv`

The `mnist_train.csv` file contains the 60,000 training examples and labels. The `mnist_test.csv` contains 10,000 test examples and labels. Each row consists of 785 values: the first value is the label (a number from 0 to 9) and the remaining 784 values are the pixel values (a number from 0 to 255).