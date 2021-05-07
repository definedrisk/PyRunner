'''
Performs a k-Mean-Clustering on the normalized price movements of the specified stocks. Outputs a text list 
with the results, together with the RGB values that were used in the line chart (see 'chart.py').
To be called via command line / C# (PythonRunner).
'''

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

import numpy as np

def create_arrays(dataframes):
	'''
	Helper function. 
	'''

	length1 = len(dataframes)
	length2 = dataframes[0].shape[0]
	movements = np.empty((length1, length2))
	labels = []

	i = 0

	for df in dataframes:

		# Check length.
		if df.shape[0] != length2:
			raise ValueError("Number of data for '{}' should be {}, but is actually {}."
								.format(df['Ticker'][0], length2, df.shape[0]))

		df['Movement'] = df['Close'] - df['Open']

		movements[i] = df['Movement'].values
		labels.append(df['Ticker'][0])

		i += 1

	return movements, labels

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

def get_rgb(color):
	'''
	Helper function: Gets the RGB values from a color name.
	'''

	r, g, b = clr.to_rgb(color)
	r *= 255
	g *= 255
	b *= 255
	rgb = "{},{},{}".format(int(r), int(g), int(b))

	return rgb

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

import pandas as pd
from sklearn.preprocessing import Normalizer
from sklearn.pipeline import make_pipeline
from sklearn.cluster import KMeans
import common
import sys, warnings
import matplotlib.colors as clr

# Suppress all kinds of warnings (this would lead to an exception on the client side).
warnings.simplefilter("ignore")

# parse command line arguments
db_path = sys.argv[1]
ticker_list = sys.argv[2]
clusters = int(sys.argv[3])
start_date = sys.argv[4]
end_date = sys.argv[5]

# Get all the required dataframes from database.
dfs = common.load_stock_data(db_path, ticker_list, start_date, end_date)

movements, tickers = create_arrays(dfs)

# Create a normalizer.
normalizer = Normalizer()

# Create a KMeans model with the specified number of clusters.
kmeans = KMeans(n_clusters = clusters)

# Make a pipeline chaining normalizer and kmeans.
pipeline = make_pipeline(normalizer, kmeans)

# Fit pipeline to the price movements.
labels = pipeline.fit_predict(movements)

# Create a DataFrame aligning labels and companies.
df = pd.DataFrame({'ticker': tickers}, index=labels)
df.sort_index(inplace=True)

# Make a real python list.
ticker_list = list(ticker_list.replace("'", "").split(','))

# Output the clusters together with the used colors
for cluster, row in df.iterrows():

	ticker = row['ticker']
	index = ticker_list.index(ticker)
	rgb = get_rgb(common.COLOR_MAP[index])

	print(cluster, ticker, rgb)

