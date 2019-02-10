import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
	selector: 'app-execution',
	templateUrl: './execution.component.html',
	styleUrls: ['./execution.component.css']
})
export class ExecutionComponent implements OnInit {

	constructor(private http:HttpClient) { }

	ngOnInit() {
	}

	uploadTemplate(files){
		if(files.length != 0){
			let template= <File>files[0];
			let formData= new FormData();
			formData.append('file',template, template.name);
			let token = localStorage.getItem("jwt");
			this.http.post('https://localhost:44396/api/users/upload',formData,{
				headers: new HttpHeaders({
					"Authorization": "Bearer " + token
				})
			}).subscribe(result =>{

			},err =>{
				console.log(err);
			});
		}
	}
}
