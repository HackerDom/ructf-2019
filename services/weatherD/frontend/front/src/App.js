import React, { Component } from "react";
import Input from "./components/Input";
import "./App.css";
import { Formik, Form, Field } from "formik";

const host = window.location.host;
const notificationsApiHost = "127.0.0.1:5000"
const rustHost = "10.33.54.127:7878"
let news = []
let articles = ["4"];
function getSources()
{
  fetch(`http://10.33.54.127:7878/get_sources_list`, {
    method: "GET",
    mode: "no-cors",
  })
  .then(async response => {
    return await response.json();
  })
  .then(body => {
    console.log("text: " + body)
    news = body.split('\n');
    console.log(news);
  });
}



class App extends Component {
  constructor(props) {
    super(props);
  }



  render() {
    //getSources();
    news = ["1", "2"]
    

    return (
        <div className="wrapper" id="wr">
          <div className="sources">
            <button id="add_source" onClick={()=> this.onSubscribeClick("add_src_popup")}>Add source</button>
            <div className="add_src_popup" id="add_src_popup">
              <button className="popup_close" onClick={() => document.getElementById("add_src_popup").style.visibility = "hidden"}>x</button>
            
              <Formik  onSubmit={this.createSource}>
            {() => (
              <Form className={"App-form"}>
                <Field name="name" component={Input} />
                <Field name="password" type="password" component={Input} />
                <Field name="isPublic" component={Input} type="checkbox"/>
                <Field name="population"  component={Input} />
                <Field name="race" component={Input} />
                <Field name="landscape" component={Input} />
                <button className="App-btn" type="submit">
                  create
                </button>

                <Field name="token"/>
              </Form>
            )}
          </Formik>
            
            </div>
            <button id="push_to_source" onClick={()=> this.onSubscribeClick("push_to_src_popup")}>Push to source</button>
            <div className="push_to_src_popup" id="push_to_src_popup">
              <button className="popup_close" onClick={() => document.getElementById("push_to_src_popup").style.visibility = "hidden"}>x</button>
              <Formik onSubmit={this.pushMessage}>
            {() => (
              <Form className={"App-form"}>
                <Field name="name" component={Input} />
                <Field name="password" type="password" component={Input} />
                <Field name="message" component={Input} />
                <button className="App-btn" type="submit">
                  push
                </button>
              </Form>
            )}
          </Formik>
            
            </div>
            <div className="sources_list">
              {news.map((t, i) => (
                  <div className="source" key={i}>
                    <span className="source_name">{t}</span>
                    <button className="subscribe_button" onClick={()=>this.onSubscribeClick("popup" + i)}>+</button>

                    <div className="popup" id={"popup" + i}>
                    <button className="popup_close" onClick={() => document.getElementById("popup" + i).style.visibility = "hidden"}>x</button>
                    <Formik onSubmit={(v) => this.subscribeOnMessage(t, v)}>
                    {() => (
                    <Form className={"App-form"}>
                    <span>{"Source:  " + t}</span>
                    <Field name="token" type="password" component={Input} />
                    <button className="App-btn" type="submit">
                      subscribe
                    </button>
                  </Form>
                  )}
                </Formik>
                    
                    </div> 
                  </div>
              ))}
            </div>
          </div>

          <div className="news">
            <h2 className="news_header">WeatherD</h2>
            <hr/>
            <div className="news_content">
              {articles.map((t, i) => (
                  <article className="article" key={i}>
                    <h4 className="source_name">{t}</h4>
                    <img className="source_image" src="ex1.jpg"/>
                    <hr/>
                  </article>
                  
              ))}
              
            </div>
          </div>
        </div>
    );
  }

onSubscribeClick = id =>
{
  var popup = document.getElementById(id);
  popup.style.visibility = "visible";
}

createSource = values =>
fetch(`http://${rustHost}/create_source`, {
  method: "post",
  mode: "no-cors",
  body: JSON.stringify({
    name: values.name,
    password: values.password,
    is_public: values.isPublic,
    population: values.population,
    race: values.race,
    landscape: values.landscape,
  })
}).then(res => res.text()).then(x => console.log("123456"));

subscribeOnMessage  = (t, values) =>
  fetch(`http://${notificationsApiHost}/subscribe?source=${t}&token=${values.token}`, {
  method: "GET",
  mode: "no-cors",
});

pushMessage = values =>
    fetch(`http://${rustHost}/push_message`, {
      method: "post",
      mode: "no-cors",
      body: JSON.stringify({
        name: values.name,
        password: values.password,
        message: values.message,
      })
    });
  };



export default App;
