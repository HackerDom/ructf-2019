import React, { Component } from "react";
import logo from "./logo.svg";
import Input from "./components/Input";
import "./App.css";
import { Formik, Form, Field } from "formik";
import debounce from "lodash.debounce"

const host = window.location.host;

class App extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    const news = ["1", "2", "3", "efsd fkjngljdng"];
    const articles = ["4", "5", "6", "32132"];
    return (
        <div className="wrapper">
          <div className="sources">
            <button id="add_source" onClick={()=> this.onSubscribeClick("add_src_popup")}>Add source</button>
            <div className="add_src_popup" id="add_src_popup">
              <button className="popup_close" onClick={() => document.getElementById("add_src_popup").style.visibility = "hidden"}>x</button>
            
              <Formik  onSubmit={this.submitNewMessage}>
            {() => (
              <Form className={"App-form"}>
                <Field name="name" component={Input} />
                <Field name="password" type="password" component={Input} />
                <button className="App-btn" type="submit">
                  create
                </button>
              </Form>
            )}
          </Formik>
            
            </div>
            <button id="push_to_source" onClick={()=> this.onSubscribeClick("push_to_src_popup")}>Push to source</button>
            <div className="push_to_src_popup" id="push_to_src_popup">
              <button className="popup_close" onClick={() => document.getElementById("push_to_src_popup").style.visibility = "hidden"}>x</button>
              <Formik onSubmit={this.submitNewMessage}>
            {() => (
              <Form className={"App-form"}>
                <Field name="name" component={Input} />
                <Field name="isPublic" component={Input} type="checkbox"/>
                <Field name="use encryption" component={Input} type="checkbox"/>
                <Field name="password" type="password" component={Input} />
                <Field name="encryption key" component={Input} />
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
                    <Formik onSubmit={() => this.submitNewMessage(t)}>
                    {() => (
                    <Form className={"Subscribe-form"}>
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



  submitNewMessage = values =>
    fetch(`http://${host}/db/${values.ch}`, {
      method: "post",
      mode: "no-cors",
      body: JSON.stringify({
        dpm: values.DpM,
        frequency: values.freq,
        text: values.text.toUpperCase(),
        need_base32: false,
        is_private: false,
        password: values.password,
      })
    });
}

export default App;
