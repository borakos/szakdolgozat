import { Injectable } from '@angular/core';
import { GrouppedTemplates, GroupData } from './interfaces';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { PARAMETERS } from '@angular/core/src/util/decorators';

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

	//Components: templates
	deleteGroup(id):Observable<boolean>{
        return this.http.delete<boolean>(this.baseUrl+"/deletegroup/"+id, {
            headers: this.headerJson
		});
	}

	//Components: edit-templates
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
	editGroup(groupData, type):Observable<boolean>{
		let credentials= JSON.stringify(groupData);
		return this.http.put<boolean>(this.baseUrl+"/edit/"+type,credentials, {
			headers: this.headerJson
		});
	}

	//Components: edit-templates
	createGroup(groupData):Observable<boolean>{
		let credentials= JSON.stringify(groupData);
		return this.http.post<boolean>(this.baseUrl+"/create",credentials, {
			headers: this.headerJson
		});
	}

	//Components: edit-templates
	removeTemplate(id, gid, setVersion):Observable<boolean>{
        return this.http.delete<boolean>(this.baseUrl+"/removetemplate/"+id+"/"+gid+"/"+setVersion, {
            headers: this.headerJson
		});
	}

	//Components: edit-templates
	uploadTemplate(formData,oid,gid,name,version):Observable<boolean>{
		let params= new HttpParams()
						.set("ownerId",oid)
						.set("groupId",gid)
						.set("name",name)
						.set("version",version);
		return this.http.post<boolean>(this.baseUrl+'/upload',formData,{params});
	}
}