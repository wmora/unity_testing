using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class OTXMLDataReader : OTTextDataReader {
			
	XmlDocument xDoc = new XmlDocument();
	/// <summary>
	/// Gets the xml of this data reader
	/// </summary>
	public XmlDocument xml
	{
		get
		{
			return xDoc;
		}
	}

	public OTXMLDataReader(string id, TextAsset txAsset) : base(id, txAsset)
	{
	}	
	
	public OTXMLDataReader(string id, string source) : base(id, source)
	{
	}	
		
	Dictionary<string, XmlNodeList> dsNodelist = new Dictionary<string, XmlNodeList>();
	protected override int LoadDataSet (string dataset, string datasource)
	{
		if (available)
		{
			XmlNodeList nodes = xml.DocumentElement.SelectNodes(datasource);
			if (nodes!=null && nodes.Count>0)
			{
				dsNodelist.Add(dataset,nodes);
				return nodes.Count;
			}			
		}
		return 0;
	}
	
	protected override object GetData (string variable)
	{
		if (dsNodelist.ContainsKey(dataset))
		{
			XmlNodeList rowNodes = dsNodelist[dataset];			
			XmlNode dataNode = rowNodes[row];
			// check if variable is an attribute of the row node
			if (dataNode.Attributes[variable]!=null)
				return dataNode.Attributes[variable].Value;
			// check if variable is a childnode
			XmlNode lNode = dataNode.SelectSingleNode(variable);
			if (lNode!=null)
				return lNode.InnerText;
		}
		return "";
	}
		
	public override bool Open()
	{
		dsNodelist.Clear();
		if (base.Open())
		{
			try
			{
				xDoc.LoadXml(text);				
				if (xDoc.DocumentElement!=null)
				{
					Available();
					return true;
				}
			}
			catch (System.Exception)
			{
			}				
		}
		_available = false;
		return false;
	}
	
	protected override void UrlLoaded(WWW www)
	{
		loadingUrl = false;
		_text = www.text;
		if (text!="")
		{
			try
			{
				xDoc.LoadXml(text);				
				if (xDoc.DocumentElement!=null)
					Available();
			}
			catch (System.Exception)
			{
			}				
		}
	}
	
	
	
}
