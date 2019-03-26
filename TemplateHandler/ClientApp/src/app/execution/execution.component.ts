import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TemplateService } from './../services/templateservice';
import { ExecutionService } from './../services/executionservice';
import { GrouppedTemplates, User, GroupData } from './../services/interfaces';
import { Observable } from 'rxjs';

@Component({
	selector: 'app-execution',
	templateUrl: './execution.component.html',
	styleUrls: ['./execution.component.css']
})
export class ExecutionComponent implements OnInit {

	fileToUpload=null;
	templateService= new TemplateService(this.http);
	executionService= new ExecutionService(this.http);
	templates: Observable<GrouppedTemplates[]>;
	user: User;
	templateData: Observable<GroupData>;
	

	constructor(private http:HttpClient) { }

	ngOnInit() {
		this.user=JSON.parse(localStorage.getItem('user'));
		this.templates=this.templateService.getGrouppedTemplates(this.user.id);
		this.templates.subscribe((response)=> {}, err => {
            console.log(err);
		});
	}

	saveDataFile(files){
		if(files.length != 0){
			this.fileToUpload=files;
		}else{
			this.fileToUpload=null;
		}
	}

	getGroup(id){
		this.templateData=this.templateService.getTemplates(id);
		this.templateData.subscribe((response)=> {}, err => {
            console.log(err);
		});
	}

	generate(version){
		this.templateData.subscribe((response : GroupData)=> {
			let i=0;
			while((i<response.templates.length) && (response.templates[i].version != version)){
				i++;
			}
			if(i<response.templates.length){
				this.uploadData(this.fileToUpload,response.templates[i].id);
			}
		}, err => {
            console.log(err);
		});
	}

	uploadData(files,tid){
		if((files != null) && (files.length != 0)){
			let dataFile= <File>files[0];
			let formData= new FormData();
			formData.append('file',dataFile, dataFile.name);
			let uploaded=this.executionService.sendToExecution(formData,tid);
			uploaded.subscribe((result) =>{},err =>{
				console.log(err);
			});
		}
	}
}
