using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base data reader class to read external data into the framework
/// </summary>
public class OTDataReader : Object {

    public delegate void DataReaderDelegate(OTDataReader reader);
	
	protected bool _available = false;
	/// <summary>
	/// This delegate is executed when the data becomes available
	/// </summary>
	public DataReaderDelegate onDataAvailable = null;

	/// <summary>
	/// If true, readers will stay open
	/// </summary>
	/// <remarks>
	/// When you are keeping the readers and register a new reader using OTDataReader.Register(newReader) and the new reader has the same id as 
	/// one that is already available the reader will re-use that already available reader.
	/// </remarks>
	static bool keepReaders = false;
	static List<OTDataReader> all = new List<OTDataReader>();
	static Dictionary<string, OTDataReader>lookup = new Dictionary<string, OTDataReader>();	
	
	/// <summary>
	/// Registeres the specified newReader.
	/// </summary>
	/// <remarks>
	/// When OTDataReader.keepReaders is true and you are registering a new reader using OTDataReader.Register(newReader) and the new reader has 
	/// the same id as one that is already available the reader will re-use that already available reader.
	/// </remarks>
	public static OTDataReader Register(OTDataReader newReader)
	{
		string id = newReader.id;
		
		if (lookup.ContainsKey(id) && !keepReaders)
			lookup[id].Close();
			
		if (!lookup.ContainsKey(id))
		{					
			all.Add(newReader);
			lookup.Add(id,newReader);
		}
		
		return lookup[id];
	}
	
	
	protected void Available()
	{
		_available = true;
		if (onDataAvailable!=null)
		  onDataAvailable(this);						
	}
	
	string _id;
	/// <summary>
	/// Gets the id of this reader
	/// </summary>
	public string id
	{
		get
		{
			return _id;
		}
	}
	
	/// <summary>
	/// Closes all readers
	/// </summary>
	static public void CloseAll()
	{
		while (all.Count>0)
		{
			if (all[0].available)
				all[0].Close();
			else
				all.RemoveAt(0);
		}
		
		all.Clear();
	}
	
	/// <summary>
	/// True if the reader is opened and available
	/// </summary>
	public bool available
	{
		get
		{
			return _available;
		}
	}
			
	/// <summary>
	/// Opens the reader
	/// </summary>
	public virtual bool Open()
	{
		_row = 0;
		_available = false;
		datasetRows.Clear();
		return _available;
	}	

	/// <summary>
	/// Closes the reader
	/// </summary>
	public virtual void Close()
	{
		if (lookup.ContainsKey(id))
			lookup.Remove(id);
		if (all.Contains(this))
			all.Remove(this);
	}	
	
	public OTDataReader(string id)
	{		
		this._id = id;			
	}
		
	protected virtual int LoadDataSet(string dataId, string dataset)
	{
		return 0;
	}

	Dictionary<string , int> datasetRows = new Dictionary<string, int>();
	/// <summary>
	/// Loads a dataset
	/// </summary>
	/// <remarks>
	/// This first row of this dataset becomes the active dataset row
	/// </remarks>
	public void DataSet(string dataset, string datasource)
	{		
		dataset = dataset.ToLower();
		int _rows = LoadDataSet(dataset, datasource);
		if (_rows>0)
		{
			datasetRows.Add(dataset,_rows);
			_dataset = dataset;
			_row = 0;
		}
		return;
	}
	
	string _dataset = "";
	/// <summary>
	/// Gets or sets the active dataset.
	/// </summary>
	public string dataset
	{
		get
		{
			return _dataset;
		}
		set
		{
			string v = value.ToLower();
			if (datasetRows.ContainsKey(v))
				_dataset = v;
			else
				_dataset = "";
		}
	}


	/// <summary>
	/// Gets or sets the active row of the current dataset
	/// </summary>
	public int rowCount
	{
		get
		{
			if (dataset == "")
				throw new System.Exception("No dataset active!");
			return datasetRows[dataset];
		}
	}
	
	int _row = 0;
	/// <summary>
	/// Gets or sets the active row of the current dataset
	/// </summary>
	public int row
	{
		get
		{
			if (dataset == "")
				throw new System.Exception("No dataset active!");
			return _row;
		}
		set
		{
			if (dataset == "")
				throw new System.Exception("No dataset active!");
			
			if (value<0 || value>=datasetRows[dataset])
				throw new System.Exception("Invalid dataset row!");
			else
			{
				_row = value;											
				_bof = false;
				_eof = false;
			}
		}
	}

	/// <summary>
	/// To first row of current dataset
	/// </summary>
	public void First()
	{
		_bof = false;
		_eof = false;
		row = 0;
	}
	/// <summary>
	/// To last row of current dataset
	/// </summary>
	public void Last()
	{
		_bof = false;
		_eof = false;
		row = rowCount-1;
	}
	/// <summary>
	/// To previous row of current dataset
	/// </summary>
	public void Previous()
	{
		if (row>1)
			row--;
		else
		{
			_bof = true;
			_row = 0;
		}
	}
	/// <summary>
	/// To next row of current dataset
	/// </summary>
	public void Next()
	{
		if (row<rowCount-1)
			row++;
		else
		{
			_eof = true;
			_row = rowCount-1;
		}
	}
	
	bool _eof = false;
	/// <summary>
	/// True if we got to the last row of the dataset
	/// </summary>
	public bool EOF
	{
		get
		{
			return _eof;
		}
	}
	
	bool _bof = false;
	/// <summary>
	/// True if we got to the first row of the dataset
	/// </summary>
	public bool BOF
	{
		get
		{
			return _bof;
		}
	}
	
	
	protected virtual object GetData(string variable)
	{
		return null;
	}
	
	object Data(string variable)
	{
		if (_dataset == "")
			throw new System.Exception("No dataset active!");
		return GetData(variable);
	}
	
	
	/// <summary>
	/// Gets data as a string from the reader
	/// </summary>
	public string StringData(string variable)
	{
		try
		{
			return System.Convert.ToString(Data(variable));
		}
		catch(System.Exception)
		{
			return "";
		}
	}
	
	/// <summary>
	/// Gets data as an integer from the reader
	/// </summary>
	public int IntData(string variable)
	{
		try
		{
			return System.Convert.ToInt32(Data(variable));
		}
		catch(System.Exception)
		{			
			return 0;
		}
	}
	
	/// <summary>
	/// Gets data as an integer from the reader
	/// </summary>
	public float FloatData(string variable)
	{
		try
		{
			return System.Convert.ToSingle(Data(variable));
		}
		catch(System.Exception)
		{			
			return 0.0f;
		}
	}

	/// <summary>
	/// Gets data as an integer from the reader
	/// </summary>
	public bool BoolData(string variable)
	{
		try
		{
			return System.Convert.ToBoolean(Data(variable));
		}
		catch(System.Exception)
		{			
			return false;
		}
	}
			
}

public class OTDataSet
{
	
}
