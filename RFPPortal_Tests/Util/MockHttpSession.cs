using System;
using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.Constants;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Linq;
    
    
public class MockHttpSession : ISession{
    readonly Dictionary<string, object> _sessionStorage = new Dictionary<string, object>();
    //public string Id => throw new NotImplementedException();

    string ISession.Id=> throw new NotImplementedException();
    bool ISession.IsAvailable=>throw new NotImplementedException();
    IEnumerable<string> ISession.Keys=>_sessionStorage.Keys;

    public void Set(string key, byte[] value)
    {
        _sessionStorage[key] = Encoding.UTF8.GetString(value);
    }

    public bool TryGetValue(string key, out byte[] value)
    {
        if (_sessionStorage[key] != null){
            value = Encoding.ASCII.GetBytes(_sessionStorage[key].ToString());
            return true;
        }
        value = null;
        return false;
    }
    void ISession.Clear(){
        _sessionStorage.Clear();
    }
    Task ISession.CommitAsync(System.Threading.CancellationToken cancellationToken){
        throw new NotImplementedException();
    }
    Task ISession.LoadAsync(System.Threading.CancellationToken cancellationToken){
        throw new NotImplementedException();
    }
    void ISession.Remove(string key){
        _sessionStorage.Remove(key);
    }
}
    