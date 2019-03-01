import { Injectable } from '@angular/core';
import { User, Role } from './interfaces';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

@Injectable()
export class UserService{

	private readonly baseUrl="https://localhost:44396/api/users";
	private headerJson:HttpHeaders;

	constructor(private http: HttpClient) { 
		this.headerJson= new HttpHeaders({
			"Content-Type": "application/json"
		});
	}

	//Components: user
	getAllUser():Observable<User[]>{
		return this.http.get<User[]>(this.baseUrl+"/index",{
			headers: this.headerJson
		});
	}

	//Components: user
	deleteUser(id):Observable<boolean>{
		return this.http.delete<boolean>(this.baseUrl+"/delete/"+id, {
			headers: this.headerJson
		})
	}

	//Components: edit-user
	getUser(id):Observable<User>{
		return this.http.get<User>(this.baseUrl+"/details/"+id,{
			headers: this.headerJson
		});
	}

	//Components: edit-user
	nameTeszt(userName):Observable<boolean>{
		return this.http.get<boolean>(this.baseUrl+"/teszt/"+userName,{
			headers: this.headerJson
		});
	}

	updateUser(user, id, controlUser):Observable<boolean>{
		console.log(user);
		delete user.repassword;
		user.role=Role[user.role];
		let credential = JSON.stringify(user);
		return this.http.put<boolean>(this.baseUrl+"/edit/"+id+"/"+controlUser.id,credential,{
			headers: this.headerJson
		});
	}

	createUser(data):Observable<boolean>{
		delete data.repassword;
		data.role=Role[data.role];
		let credentials = JSON.stringify(data);
		return this.http.post<boolean>(this.baseUrl+"/create", credentials, {
			headers: this.headerJson
		});
	}
}