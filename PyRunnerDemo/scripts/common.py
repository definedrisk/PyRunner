"""
Common functions and a colormap for the line charts (see 'chart.py').
"""

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

from sqlalchemy import create_engine
import pandas as pd

def load_stock_data(db, tickers, start_date, end_date):
    """
    Loads the stock data for the specified ticker symbols, and for the specified date range.
    :param db: Full path to database with stock data.
    :param tickers: A list with ticker symbols.
    :param start_date: The start date.
    :param end_date: The start date.
    :return: A list of time-indexed dataframe, one for each ticker, ordered by date.
    """

    SQL = "SELECT * FROM Quotes WHERE TICKER IN ({}) AND Date >= '{}' AND Date <= '{}'"\
          .format(tickers, start_date, end_date)

    engine = create_engine('sqlite:///' + db)

    df_all = pd.read_sql(SQL, engine, index_col='Date', parse_dates='Date')
    df_all = df_all.round(2)

    result = []

    for ticker in tickers.split(","):
        df_ticker = df_all.query("Ticker == " + ticker)
        result.append(df_ticker)

    return result

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

import io, sys, base64

def print_figure(fig):
	"""
	Converts a figure (as created e.g. with matplotlib or seaborn) to a png image and this 
	png subsequently to a base64-string, then prints the resulting string to the console.
	"""
	
	buf = io.BytesIO()
	fig.savefig(buf, format='png')
	print(base64.b64encode(buf.getbuffer()))

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

# Custom colormap that is used with line charts
COLOR_MAP = [
	'blue', 'orange', 'green', 'red', 'purple', 'brown', 'pink', 'gray', 'olive', 'cyan',
	'darkblue', 'darkorange', 'darkgreen', 'darkred', 'rebeccapurple', 'darkslategray', 
	'mediumvioletred', 'dimgray', 'seagreen', 'darkcyan', 'deepskyblue', 'yellow', 
	'lightgreen', 'lightcoral', 'plum', 'lightslategrey', 'lightpink', 'lightgray', 
	'lime', 'cadetblue'
	]

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
