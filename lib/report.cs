/*
  CTM Reports API

  Reports API is a batch API, meaning you must have a web server available to receive queued report jobs

  Your service can send a request to CTM asking for some range of data and CTM will respond back to your service's web endpoint with the result data.

  POST /api/v1/accounts/:account_id/reports/batch

  Send a batch Job to retrieve report data from the given account

  A sample report would be to POST JSON such as the following:

  query=

  "reports": [
    {
      "name": "calls by source report",
      "account": 1,
      "type":"sum",
      "dimension": "source",
      "metric": "call",
      "filters": {
        "start_date": "2012-08-01",
        "end_date": "2012-08-15"
      }
    },
    {
      "name": "calls by day per source report",
      "account": 2,
      "type":"series",
      "dimension": "source",
      "metric": "call",
      "filters": {
        "start_date": "2012-08-01",
        "end_date": "2012-08-15",
        "source": "Google Paid"
      }
    }
  ]

  Each report query has the following fields:

  name:
    a descriptive name you can provide to identify the given report

  account:
    the account id to run the report under

  type:
    how the data should be collected
      - sum: sum the data points
      - series: a single data point per day between the start and end date

  dimension: 
    A component of the call to group by.  For the given filter how should we group results.
      - source
      - tracking_number
      - receiving_number

  metric:
    Over which set of data points should we run the report.
      - call: all calls
      - caller: only unique callers e.g. not duplicate calls

  start_date:
    The first day inclusively to consider for the report data

  end_date:
    The last day inclusively to consider for the report data

  filters:
    In addition to the date range you may also further refine your queries by the following attributes of a call

      - tag_list: one or more list of tags 
        e.g. ['tag1','tag2',...]
      - tracking_number: one or more specific tracking
        e.g. ['15553334444','15553335555',...] - note numbers should include the country prefix
      - receiving_number: one or more specific receiving numbers
      - ring_time: an integer value representing the number of seconds of ring time for the call and a comparision operator 
        e.g. {"seconds":55, "compare": ">="} or {"seconds":23, "compare": "<="} to get all calls that rang longer than 55 seconds or all calls that rang less than 23 seconds
      - talk_time: an integer value representing the number of seconds of talk time for the call and a comparision operator
			- source: name of one of the tracking sources you have defined e.g. "Google Paid"

    Each report is limited to no more than than 2 filters per report.

  query1:
    this will return the calls aggregated by source e.g.
    {"Google Paid": 61, "Google Orgnaic": 46}

  query2:
    this will return a series of points between start_date and end_date of calls by source

    "series": [{"2012-08-01": {"Google Paid':23, "Google Orgnaic":18},
                "2012-08-02": {"Google Paid':38, "Google Orgnaic":28},
                ...
              }]

*/
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Json;
using System.Collections.Generic;

namespace CTM {
	public class Filter {
		public string key;
		public string val;
		public Filter(string key, string val) {
			this.key = key;
			this.val = val;
		}
	}

  public class Report {
		public string name;
		public string type;
		public string dimension;
		public string metric;
		public string callbackURL;
		public Filter[] filters;

		public Report(string name, string type, string dimension, string metric, string callbackURL, Filter[] filters) {
			this.name = name;
			this.type = type;
			this.dimension = dimension;
			this.metric = metric;
			this.callbackURL = callbackURL;
			this.filters = filters;
		}

		public System.Json.JsonObject JSONFilters(int account_id) {
			System.Json.JsonObject filters = new System.Json.JsonObject();
			filters.Add("name", (JsonValue)this.name);
			filters.Add("type", (JsonValue)this.type);
			filters.Add("dimension", (JsonValue)this.dimension);
			filters.Add("metric", (JsonValue)this.metric);
			filters.Add("callback_url", (JsonValue)this.callbackURL);
			filters.Add("account", (JsonValue)account_id);
      System.Json.JsonObject filter_key = new System.Json.JsonObject();

			foreach(Filter filter in this.filters) {
				filter_key.Add(filter.key, (JsonValue)filter.val);
			}

      filters.Add("filters", filter_key);

			return filters;
		}

    public static int Query(AuthToken token, Report[] reports) {
			System.Json.JsonArray json_reports = new System.Json.JsonArray();
			foreach(Report report in reports) {
				json_reports.Add((JsonValue)report.JSONFilters(token.account_id));
			}
			System.Json.JsonObject query = new System.Json.JsonObject();
			query.Add("reports", (JsonValue)json_reports);
			Console.WriteLine(query.ToString());
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/reports/batch.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["query"] = query.ToString();
      CTM.Response res = request.post(parameters);
      if ((string)res.data["status"] == "success") {
				return reports.Length;
      } else {
        return 0;
      }
		}
  }
}
