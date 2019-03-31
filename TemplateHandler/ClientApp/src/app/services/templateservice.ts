import { Injectable } from '@angular/core';
import { GrouppedTemplates, GroupData, UserGroup } from './interfaces';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

@Injectable()
export class TemplateService{

	private readonly baseUrl="https://localhost:44396/api/templatefiles";
	private headerJson:HttpHeaders;

	constructor(private http: HttpClient) { 
		this.headerJson= new HttpHeaders({
			"Content-Type": "application/json"
		});
	}

	//Components: templates
	getGrouppedTemplates(id):Observable<GrouppedTemplates[]>{
        return this.http.get<GrouppedTemplates[]>(this.baseUrl+"/index/"+id, {
            headers: this.headerJson
		});
	}

	//Components: execution
	getGrouppedTemplatesExecution(id):Observable<GrouppedTemplates[]>{
        return this.http.get<GrouppedTemplates[]>(this.baseUrl+"/indexexecution/"+id, {
            headers: this.headerJson
		});
	}

	//Components: templates
	deleteGroup(id):Observable<boolean>{
        return this.http.delete<boolean>(this.baseUrl+"/deletegroup/"+id, {
            headers: this.headerJson
		});
	}

	//Components: edit-templates, execution
	getTemplates(id):Observable<GroupData>{
        return this.http.get<GroupData>(this.baseUrl+"/details/"+id, {
            headers: this.headerJson
		});
	}

	//Components: edit-templates
	nameTeszt(groupName, id):Observable<boolean>{
        return this.http.get<boolean>(this.baseUrl+"/teszt/"+groupName+"/"+id, {
            headers: this.headerJson
		});
	}

	//Components: edit-templates
	editGroup(type, gid, description, gname, oid, defversion, tname=null, ttype=null, file=null):Observable<boolean>{
		let params= new HttpParams()
						.set("groupId",gid)
						.set("description",description)
						.set("groupName",gname)
						.set("owner",oid)
						.set("defaultVersion",defversion)
						.set("templateName",tname)
						.set("templateType",ttype);
		return this.http.put<boolean>(this.baseUrl+"/edit/"+type,file,{params});
	}

	//Components: edit-templates
	createGroup(groupData):Observable<boolean>{
		let credentials= JSON.stringify(groupData);
		return this.http.post<boolean>(this.baseUrl+"/create",credentials, {
			headers: this.headerJson
		});
	}

	//Components: edit-templates
	getUserGroups(id):Observable<UserGroup[]>{
		return this.http.get<UserGroup[]>(this.baseUrl+"/usergroups/"+id, {
			headers: this.headerJson
		});
	}

	//Components: edit-templates
	removeTemplate(id):Observable<boolean>{
        return this.http.delete<boolean>(this.baseUrl+"/removetemplate/"+id, {
            headers: this.headerJson
		});
	}
}