"""
Outputs a line chart including all the selected stocks.
To be called via command line / C# (PythonRunner).
"""

import common
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
from matplotlib import style
from cycler import cycler
import io, sys, base64
import warnings

# Suppress all kinds of warnings (this would lead to an exception on the client side).
warnings.simplefilter("ignore")

# Preconfig plotting style, line colors and chart size.
style.use('ggplot')
plt.figure(figsize=(9, 6))
plt.rc('axes', prop_cycle=(cycler('color', common.COLOR_MAP)))

# parse command line arguments
db_path = sys.argv[1]
tickers = sys.argv[2]
start_date = sys.argv[3]
end_date = sys.argv[4]

# Get all the dataframes from database.
dfs = common.load_stock_data(db_path, tickers, start_date, end_date)

# Draw a line to the chart for every single stock.
for df in dfs:

    # Normalize the close prices such that the first value is always zero and the scale is percent.
    initial_value = df['Close'][0]
    df['NormalizedClose'] = (df['Close'] - initial_value) / initial_value * 100
    plt.plot(df['NormalizedClose'], linewidth=1)

# Configure chart labels
plt.axes().xaxis.set_major_formatter(mdates.DateFormatter("%Y-%m"))
plt.xticks(rotation=30)
plt.ylabel('Change (%)')

# Finally, print the chart as base64 string to the console.
common.print_figure(plt.gcf())


