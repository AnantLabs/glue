enum HttpHeaderType
{
    ContentType,
    ContentDisposition,
    ContentID,
    ContentLength,
    KeepAlive,
    CloseConnection,
    CacheControl,
    GetCookie,
    Cookie
}

class HttpHeader
{
    public static HttpHeader Parse(byte[] data, int offset, int length, out int read);
    public static HttpHeader Parse(string data);

    private string[] attrNames;
    private string[] attrValues;
    private byte[] data;
    
    public string Name;
    public HttpHeaderType Known;
    public string Value;
    public int AttributeCount
    {
        get { return attrNames.Length; }
    }
    public string this[int i]
    {
        get { return GetAttributeValue(i); }
        set { SetAttributeValue(i, value); }
    }
    public string this[string name]
    {
        get { return GetAttributeValue(name); }
        set { SetAttributeValue(name, value); }
    }
    public string IndexOfAttribute(string name)
    {
        for (int i = 0; i < attrNames.Length; i++)
            if (string.Compare(name, attrNames[i], true) == 0)
                return i;
        return -1;
    }
    public string GetAttributeCount()
    {
        return attrNames.Length;
    }
    public string GetAttributeName(int i)
    {
        return attrNames[i];
    }
    public string GetAttributeValue(int i)
    {
        return attrValues[i];
    }
    public string GetAttributeValue(string name)
    {
        int i = IndexOfAttribute(name);
        if (i < 0)
            return null;
        else
            return GetAttributeValue(i);
    }
    public void SetAttributeValue(int i, string value)
    {
        attrValues[i] = value;
    }
    public void SetAttributeValue(string name, string value)
    {
        int i = IndexOfAttribute(name);
        if (i < 0)
        {
            i = attrValues.Length;
            attrValues = GrowArray(attrValues, 1);
            attrNames = GrowArray(attrNames, 1);
            attrNames[i] = name;
        }
        attrValues[i] = value;
    }
}

class HttpHeaderCollection
{
    public Add(HttpHeader);
    public int Count;
    public HttpHeader this[int i];
    public HttpHeader this[string name];
    public HttpHeader this[HttpHeaderType type];
}
class HttpMessage
{
    public HeaderCollection Headers;
    public byte[] Header;
    public byte[] Body;
}

int GetContentLength(HttpMessage msg)
{
    HttpHeader cl = msg.Headers[HttpHeaderType.ContentLength];
    if (cl == null)
        return -1;
    else
        return Convert.ToInt32(cl.Value);
}

HttpMessage message = HttpMessage.Parse(ReadHeaders());
HttpHeader ct = message.Headers[HttpHeader.ContentType];
HttpHeader cl = message.Headers[HttpHeader.ContentLength];
if (ct.Value == "multipart/form-data")
{
    string boundary = ct["boundary"];
    byte[] data = ReadHeader(boundary);
    while (data != null)
    {
        HttpMessage part = HttpMessage.Parse(data);
        if (part.Headers[HttpHeader.ContentLength] != null)
            part.Body = ReadBody(Convert.ToInt32(part.Headers[HttpHeader.ContentLength].Value));
        else
            part.Body = ReadBody();
        data = ReadHeader(boundary);
    }
}
else if (ct.Value == "application/x-www-form-urlencoded)
{
    if (cl != null)
        message.Body = ReadBody(Convert.ToInt32(cl.Value));
    else
        message.Body = ReadBody();
}
else
{
    if (cl != null)
        message.Body = ReadBody(Convert.ToInt32(cl.Value));
    else
        message.Body = ReadBody();
}





ReadHeaderPart();
int offset = 0;
int read = 0;
ParseRequestLine(data, offset, data.Length - offset, out read);
offset += read;
HttpHeader header;
while ((header = HttpHeader.Parse(data, offset, data.Length - offset, out read) != null)
{
    // store header
    offset += read;
}
HttpHeader ct = knownRequestHeaders[HttpHeaderEnum.ContentType];
if (ct.Value == "application/x-www-form-urlencoded")
{
    ReadBodyPart();
}
else if (ct.Value = "multipart/form-data")
{
    string boundary = ct["boundary"];
    while (ReadHeaderPart(boundary))
    {
        HttpHeader h;
        HttpHeader ct;
        HttpHeader cl;
        HttpHeader cd;
        HttpHeader cid;
        while ((h = HttpHeader.Parse(data, offset, data.Length - offset, out read) != null)
        {
            if (h.Known = HttpHeaderType.ContentType)
                ct = h;
            else if (h.Known = HttpHeaderType.ContentDisposition)
                cd = h;
            else if (h.Known = HttpHeaderType.ContentLength)
                cl = h;
            else if (h.Known = HttpHeaderType.ContentID)
                cid = h;
            offset += read;
        }
        if (cl != null)
            ReadBodyPart(GetEncoding(ct), int.Parse(cl.Value));
        else
            ReadBodyPart(GetEncoding(ct));
        HttpBody body = new HttpBody(ct, cl, cd, cid, data, 0, data.Length);
        list.Add(body);
    }
    
}
