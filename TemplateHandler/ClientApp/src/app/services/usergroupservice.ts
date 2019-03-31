import { Injectable } from '@angular/core';
import { User, Role, UserGroup, UserGroupMember } from './interfaces';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

@Injectable()
export class UserGroupService{

	private readonly baseUrl="https://localhost:44396/api/usergroups";
	private headerJson:HttpHeaders;

	constructor(private http: HttpClient) { 
		this.headerJson= new HttpHeaders({
			"Content-Type": "application/json"
		});
	}

	//Components: usergroups
	getAllUserGroup():Observable<UserGroup[]>{
		return this.http.get<UserGroup[]>(this.baseUrl+"/index",{
			headers: this.headerJson
		});
	}

	//Components: usergroups
	deleteUserGroup(id):Observable<boolean>{
		return this.http.delete<boolean>(this.baseUrl+"/delete/"+id, {
			headers: this.headerJson
		})
	}

	//Components: edit-usergroup
	getUserGroup(id):Observable<UserGroup>{
		return this.http.get<UserGroup>(this.baseUrl+"/details/"+id,{
			headers: this.headerJson
		});
	}

	//Components: edit-usergroup
	getUserGroupMembers(id):Observable<UserGroupMember[]>{
		return this.http.get<UserGroupMember[]>(this.baseUrl+"/members/"+id,{
			headers: this.headerJson
		});
	}

	//Components: edit-usergroup
	nameTeszt(groupName):Observable<boolean>{
		return this.http.get<boolean>(this.baseUrl+"/teszt/"+groupName,{
			headers: this.headerJson
		});
	}

	//Components: edit-usergroup
	editGroup(type, gid, description, gname, uid=null, rights=null):Observable<boolean>{
		let params= new HttpParams()
						.set("groupId",gid)
						.set("description",description)
						.set("groupName",gname)
						.set("userId",uid)
						.set("rights",rights);
		return this.http.put<boolean>(this.baseUrl+"/edit/"+type,"Add member",{
			headers: this.headerJson,
			params
		});
	}

	//Components: edit-usergroup
	createGroup(data):Observable<boolean>{
		let credentials = JSON.stringify(data);
		return this.http.post<boolean>(this.baseUrl+"/create", credentials, {
			headers: this.headerJson
		});
	}

	//Components: edit-usergroups
	removeUser(id):Observable<boolean>{
        return this.http.delete<boolean>(this.baseUrl+"/removeuser/"+id, {
            headers: this.headerJson
		});
	}
}