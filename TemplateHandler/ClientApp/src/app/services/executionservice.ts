import { Injectable } from '@angular/core';
import { GrouppedTemplates, GroupData } from './interfaces';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { PARAMETERS } from '@angular/core/src/util/decorators';

@Injectable()
export class ExecutionService{

	private readonly baseUrl="https://localhost:44396/api/execution";
	private headerJson:HttpHeaders;

	constructor(private http: HttpClient) { 
		this.headerJson= new HttpHeaders({
			"Content-Type": "application/json"
		});
	}

	//Components: execute
	sendToExecution(formData,tid):Observable<Blob>{
		let params= new HttpParams()
						.set("templateId",tid);
		return this.http.post<Blob>(this.baseUrl+'/execute',formData,{params, responseType: 'blob' as 'json'});
	}
}