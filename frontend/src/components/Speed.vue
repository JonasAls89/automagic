<template>
    <div id="Speed" v-on:keyup.enter="insertData">
        <h1>Automagic</h1>
        <input name="Dbase" ref='Dbase' value="Type in your database type. Supported database types: MySQL and PostgreSQL">
        <input name="dbHost" ref='dbHost' value="Type in your IP address for connecting to your chosen database">
        <input name="dbPort" ref='dbPort' value="Type in your Port for connecting to your chosen database">
        <input name="dbName" ref='dbName' value="Type in the name of your database">
        <input name="dbUser" ref='dbUser' value="Type in the username required for connecting">
        <input name="MappingChoice" ref='MappingChoice' value="Type in whether you'd like 'index' or 'fkey' mapping">
        <h5 v-if='isFirstCommit'>Type in your password in the field below</h5>
        <input name="dbPassword" ref='dbPassword' value="Type in the password as well" type="password"> 
        <br>
        <button v-on:click.prevent="insertData">Submit values and run scan of database</button>
        <br>
        <img v-if='isLoading' src="https://media.giphy.com/media/3oEjI6SIIHBdRxXI40/giphy.gif" alt="Loading GIF">
        <h2 v-if="isTableVis">Found Tables</h2>
        <p v-if="isTableVis">{{ rows }}</p>
        <h2 v-if="isTableVis">----</h2>
        <br>
        <p v-if="isTableVis">To make pipes in SESAM from the tables listed above, provide the two values below and press the button</p>
        <br>
        <h5 v-if="isTableVis">Type in your SESAM access token in the below field. The token is created in the 'Subscription' section in Sesam, remember to set privileges to admin</h5>
        <input v-if="isTableVis" id="JWT" name="SesamJWT" ref='SesamJWT' value="testing for foobar" type="password">
        <br>
        <h5 v-if="isTableVis">Type in the subscription ID of your Sesam Client in the below field</h5>
        <input v-if="isTableVis" id="SubID" name="SesamSubID" ref='SesamSubID' value="testing for foobar" type="password">
        <br>
        <button v-if="isTableVis" v-on:click.prevent="dataToSesam">Make pipes in SESAM</button>
        <br>
        <br>
        <p v-if="isButtonActivated">Thank you for your time!</p> 
        <p v-if="isButtonActivated">You can now go to SESAM to look at your newly created pipes.</p>
    </div>
</template>

<script>
import api from '../api'
export default {
    name: 'Speed',
    data: () => {
        return {
            isTableVis : false,
            isFirstCommit : true,
            isButtonActivated: false,
            isLoading : false,
            rows : {
                tables: '{{tables}}'
            }  
        }
    },
    methods: {
        async insertData(){ 
          let Dbase = this.$refs.Dbase.value
          let dbHost = this.$refs.dbHost.value
          let dbPort = this.$refs.dbPort.value
          let dbName = this.$refs.dbName.value
          let dbUser = this.$refs.dbUser.value
          let dbPassword = this.$refs.dbPassword.value
          let MappingChoice = this.$refs.MappingChoice.value
          this.isLoading = true
          await fetch('http://localhost:56886/api/v2/job/create', {
            method: 'POST',
            headers: {
              'Accept': 'application/json',
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Dbase: Dbase,
                dbHost: dbHost,
                dbPort: dbPort,
                dbName: dbName,
                dbUser: dbUser,
                dbPassword: dbPassword,
                MappingChoice: MappingChoice
            })
        });
        this.$refs.Dbase.value = []
        this.$refs.dbHost.value = []
        this.$refs.dbPort.value = []
        this.$refs.dbName.value = []
        this.$refs.dbUser.value = []
        this.$refs.dbPassword.value = []
        this.$refs.MappingChoice.value = []
        this.pasteData()
        },
        pasteData(){
            api.getResource('/api/v2/tables')
                .then((data) => {
                // eslint-disable-next-line no-console
                //console.log(data)
            if(data != null && data != '') {
                this.isTableVis = true    
                this.rows = data
                //console.log(data)
                this.isLoading = false
                this.isFirstCommit = false
            }})
        },
        dataToSesam(){
          let SesamJWT = this.$refs.SesamJWT.value
          let SesamSubID = this.$refs.SesamSubID.value
          this.isButtonActivated = true
          fetch('http://localhost:56886/api/v2/pipes', {
            method: 'POST',
            headers: {
              'Accept': 'application/json',
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                SesamJWT: SesamJWT,
                SesamSubID: SesamSubID
            })
        });
        this.$refs.SesamJWT.value = []
        this.$refs.SesamSubID.value = []
        this.$refs.rows.value = []
        }
    }    
}
</script>

<style scoped>
h1 {
    color: blue;
}
table {
    margin: auto;
    font-family: 'Avenir', Helvetica, Arial, sans-serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    text-align: center;
    margin: center;
}

#JWT, #SubID{
      text-align: center;
      width:50%;
      height:30px;
      padding:5px 10px;
      font-size: 12px;
      color: black;
      letter-spacing:1px;
      background: #FFF;
      border:2px solid #FFF;
      margin-bottom:25px;
      -webkit-transition:all .1s ease-in-out;
      -moz-transition:all .1s ease-in-out;
      -ms-transition:all .1s ease-in-out;
      -o-transition:all .1s ease-in-out;
      transition:all .1s ease-in-out;
}

table th {
  text-transform: uppercase;
  font-size: 17px;
  color: blue;
  padding: 10px;
  min-width: 30px;
  border-bottom: 2px solid #7D82A8;
  border: 2px;
}

table td {
  padding: 8px;
}

h5 {
   color: rgb(87, 70, 70); 
}

input {
      text-align: center;
      width:30%;
      height:30px;
      padding:5px 10px;
      font-size: 12px;
      color: rgba(0, 0, 0);
      letter-spacing:1px;
      background: #FFF;
      border:2px solid #FFF;
      margin-bottom:25px;
      -webkit-transition:all .1s ease-in-out;
      -moz-transition:all .1s ease-in-out;
      -ms-transition:all .1s ease-in-out;
      -o-transition:all .1s ease-in-out;
      transition:all .1s ease-in-out;
}

button {
      width:20%;
      padding:5px 10px;
      font-size: 12px;
      letter-spacing:1px;
      background:blue;
      height:40px;
      text-transform:uppercase;
      letter-spacing:1px;
      color:#FFF;
      -webkit-transition:all .1s ease-in-out;
      -moz-transition:all .1s ease-in-out;
      -ms-transition:all .1s ease-in-out;
      -o-transition:all .1s ease-in-out;
      transition:all .1s ease-in-out;
}

</style>
